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
            _logger.LogInformation("Execute transfer order event");

            var ev = JsonSerializer.Deserialize<TransferOrderEvent>(messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ev == null)
            {
                _logger.LogWarning("TransferOrderEvent deserialization failed");
                return;
            }

            _logger.LogInformation("Handling TransferOrderEvent: {@Event}", ev);

            bool success = true;
            string? reason = null;

            try
            {
                if (ev.DealerId == null || ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    success = false;
                    reason = "Missing required fields: DealerId/VehicleVersionId/Quantity.";
                }
                else
                {
                    var status = ev.Status?.Trim().ToLowerInvariant();
                    int delta = 0;

                    switch (status)
                    {
                        case "shipping":
                            delta = -ev.Quantity.Value;
                            _logger.LogInformation(
                                "Status=shipping → Decrease dealer stock by {AbsDelta} for DealerId={DealerId}",
                                Math.Abs(delta), ev.DealerId);
                            break;

                        case "received":
                            delta = ev.Quantity.Value;
                            _logger.LogInformation(
                                "Status=received → Increase dealer stock by {Delta} for DealerId={DealerId}",
                                delta, ev.DealerId);
                            break;

                        default:
                            _logger.LogInformation("Status={Status} → No stock change", ev.Status);
                            break;
                    }

                    if (delta != 0)
                    {
                        var updated = await _inventoryService.UpdateDealerStockAsync(
                            ev.DealerId.Value,
                            ev.VehicleVersionId.Value,
                            delta
                        );

                        if (!updated)
                        {
                            success = false;
                            reason = "Failed to update dealer stock (not found or insufficient stock).";
                            _logger.LogWarning(
                                "Update dealer stock failed. DealerId={DealerId}, VersionId={VersionId}, Delta={Delta}",
                                ev.DealerId, ev.VehicleVersionId, delta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                reason = ex.Message;
                _logger.LogError(ex, "Failed to handle TransferOrderEvent {@Event}", ev);
            }

            var confirm = new ConfirmOrderEvent
            {
                OrderId = ev.OrderId,
                OrderType = "transfer",
                DealerId = ev.DealerId,
                Status = ev.Status,
                IsApproved = success,
                Reason = reason
            };

            await _producer.PublishAsync("confirm-topic", ev.DealerId?.ToString() ?? "unknown", confirm, ct);
            _logger.LogInformation(
                "Sent ConfirmOrderEvent for Transfer {OrderId}, Success={Success}",
                ev.OrderId, success);
        }
    }
}
