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
                _logger.LogWarning(" AllocationOrderEvent deserialization failed");
                return;
            }

            _logger.LogInformation("Handling AllocationOrderEvent...");

            bool success = true;
            string? reason = null;

            try
            {
                if (ev.VehicleVersionId == null || ev.Quantity == null)
                    throw new ArgumentException("Missing VehicleVersionId or Quantity.");

                if (!ev.IsRecived)
                {
                    await _brandInventoryService.UpdateStockAsync(ev.VehicleVersionId.Value, -ev.Quantity.Value);
                }
                else
                {
                    if (ev.DealerId == null)
                        throw new ArgumentException("DealerId required for received allocation.");

                    await _inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId.Value, ev.Quantity.Value);
                }
            }
            catch (Exception ex)
            {
                success = false;
                reason = ex.Message;
                _logger.LogError(ex, " Failed to handle AllocationOrderEvent {@Event}", ev);
            }

            var confirm = new ConfirmOrderEvent
            {
                OrderId = ev.OrderId,
                OrderType = "allocation",
                DealerId = ev.DealerId,
                IsApproved = success,
                Reason = reason
            };

            await _producer.PublishAsync("confirm-topic", ev.DealerId?.ToString() ?? "unknown", confirm, ct);
            _logger.LogInformation(" Sent ConfirmOrderEvent for Allocation {OrderId}, Success={Success}", ev.OrderId, success);
        }
    }
}
