using System.Text.Json;
using BrandService.Infrastructure.Services;
using BrandService.Kafka.Event;
using BrandService.Kafka.Messaging.Producer;

namespace BrandService.Kafka.Handler
{
    public class AllocationOrderHandler : IOrderEventHandler
    {
        private readonly ILogger<AllocationOrderHandler> _logger;
        private readonly BrandInventoryService _brandInventoryService;
        private readonly InventoryService _inventoryService;
        private readonly IKafkaProducer _producer;

        public string Topic => "allocation-topic";

        public AllocationOrderHandler(
            ILogger<AllocationOrderHandler> logger,
            BrandInventoryService brandInventoryService,
            InventoryService inventoryService,
            IKafkaProducer producer)
        {
            _logger = logger;
            _brandInventoryService = brandInventoryService;
            _inventoryService = inventoryService;
            _producer = producer;
        }

        public async Task HandleAsync(string messageValue, CancellationToken ct)
        {
            var ev = JsonSerializer.Deserialize<AllocationOrderEvent>(messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ev == null)
            {
                _logger.LogWarning("AllocationOrderEvent deserialization failed");
                return;
            }

            _logger.LogInformation("Handling AllocationOrderEvent: {@Event}", ev);

            bool success = true;
            string? reason = null;

            try
            {
                if (ev.VehicleVersionId == null || ev.Quantity == null)
                    throw new ArgumentException("Missing VehicleVersionId or Quantity.");

                int delta = 0;
                var status = ev.Status?.Trim().ToLowerInvariant();

                switch (status)
                {
                    case "shipping":
                        delta = -ev.Quantity.Value;
                        _logger.LogInformation("Status=shipping → Decrease stock {Delta} for Brand",
                            delta);
                        break;

                    case "received":
                        delta = ev.Quantity.Value;
                        _logger.LogInformation("Status=received → Increase stock {Delta} for DealerId={DealerId}",
                            delta, ev.DealerId);
                        break;

                    default:
                        _logger.LogInformation("Status={Status} → Unchange stock", ev.Status);
                        break;
                }

                if (delta > 0)
                {
                    await _inventoryService.UpdateDealerStockAsync(
                        ev.DealerId.Value,
                        ev.VehicleVersionId.Value,
                        delta
                    );
                } else if (delta < 0)
                {
                    await _brandInventoryService.UpdateStockAsync(ev.VehicleVersionId.Value, delta);
                }
            }
            catch (Exception ex)
            {
                success = false;
                reason = ex.Message;
                _logger.LogError(ex, "Failed to handle AllocationOrderEvent {@Event}", ev);
            }

            var confirm = new ConfirmOrderEvent
            {
                OrderId = ev.OrderId,
                OrderType = "allocation",
                DealerId = ev.DealerId,
                Status = ev.Status,
                IsApproved = success,
                Reason = reason
            };

            await _producer.PublishAsync("confirm-topic", ev.DealerId?.ToString() ?? "unknown", confirm, ct);
            _logger.LogInformation("Sent ConfirmOrderEvent for Allocation {OrderId}, Success={Success}", ev.OrderId,
                success);
        }

    }
}
