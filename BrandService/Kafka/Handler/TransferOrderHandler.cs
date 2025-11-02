using System.Text.Json;
using BrandService.Infrastructure.Services;
using BrandService.Kafka.Event;
using BrandService.Kafka.Messaging.Producer;

namespace BrandService.Kafka.Handler
{
    public class TransferOrderHandler : IOrderEventHandler
    {
        private readonly ILogger<TransferOrderHandler> _logger;
        private readonly InventoryService _inventoryService;
        private readonly IKafkaProducer _producer;

        public string Topic => "transfer-topic";

        public TransferOrderHandler(
            ILogger<TransferOrderHandler> logger,
            InventoryService inventoryService,
            IKafkaProducer producer)
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _producer = producer;
        }

        public async Task HandleAsync(string messageValue, CancellationToken ct)
        {
            var ev = JsonSerializer.Deserialize<TransferOrderEvent>(messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ev == null)
            {
                _logger.LogWarning(" TransferOrderEvent deserialization failed");
                return;
            }

            _logger.LogInformation("Handling TransferOrderEvent...");

            bool success = true;
            string? reason = null;

            try
            {
                if (ev.DealerId == null || ev.VehicleVersionId == null || ev.Quantity == null)
                    throw new ArgumentException("Missing required fields in TransferOrderEvent.");

                var delta = ev.IsRecived ? ev.Quantity.Value : -ev.Quantity.Value;
                await _inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId.Value, delta);

                _logger.LogInformation(" Dealer stock updated: {DealerId}, Δ={Delta}", ev.DealerId, delta);
            }
            catch (Exception ex)
            {
                success = false;
                reason = ex.Message;
                _logger.LogError(ex, " Failed to handle TransferOrderEvent {@Event}", ev);
            }

            var confirm = new ConfirmOrderEvent
            {
                OrderId = ev.OrderId,
                OrderType = "transfer",
                DealerId = ev.DealerId,
                IsApproved = success,
                Reason = reason
            };

            await _producer.PublishAsync("confirm-topic", ev.DealerId?.ToString() ?? "unknown", confirm, ct);
            _logger.LogInformation(" Sent ConfirmOrderEvent for Transfer {OrderId}, Success={Success}", ev.OrderId, success);
        }
    }
}
