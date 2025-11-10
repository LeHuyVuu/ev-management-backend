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
                // Validate input
                if (ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    success = false;
                    reason = "Missing VehicleVersionId or Quantity.";
                }
                else
                {
                    var status = ev.Status?.Trim().ToLowerInvariant();
                    int delta = 0;

                    switch (status)
                    {
                        case "shipping":
                            delta = -ev.Quantity.Value; // trừ kho brand
                            _logger.LogInformation("Status=shipping → Decrease brand stock by {Delta}", -delta);
                            break;

                        case "received":
                            delta = ev.Quantity.Value; // cộng kho dealer
                            _logger.LogInformation(
                                "Status=received → Increase dealer stock by {Delta} for DealerId={DealerId}",
                                delta, ev.DealerId);
                            break;

                        default:
                            _logger.LogInformation("Status={Status} → No stock change", ev.Status);
                            break;
                    }

                    // Thực hiện update theo delta, dùng bool để biết thành/bại
                    if (delta > 0)
                    {
                        // Cộng kho dealer
                        if (ev.DealerId == null)
                        {
                            success = false;
                            reason = "DealerId is required when increasing dealer stock.";
                        }
                        else
                        {
                            var updated = await _inventoryService.UpdateDealerStockAsync(
                                ev.DealerId.Value,
                                ev.VehicleVersionId.Value,
                                delta
                            );

                            if (!updated)
                            {
                                success = false;
                                reason = "Failed to increase dealer stock (not found or invalid state).";
                                _logger.LogWarning(
                                    "Increase dealer stock failed. DealerId={DealerId}, VersionId={VersionId}, Delta={Delta}",
                                    ev.DealerId, ev.VehicleVersionId, delta);
                            }
                        }
                    }
                    else if (delta < 0)
                    {
                        // Trừ kho brand
                        var updated = await _brandInventoryService.UpdateStockAsync(
                            ev.VehicleVersionId.Value,
                            delta
                        );

                        if (!updated)
                        {
                            success = false;
                            reason = "Failed to decrease brand stock (insufficient stock or not found).";
                            _logger.LogWarning("Decrease brand stock failed. VersionId={VersionId}, Delta={Delta}",
                                ev.VehicleVersionId, delta);
                        }
                    }
                    // delta == 0 → không đổi kho, success giữ nguyên
                }
            }
            catch (Exception ex)
            {
                // Lỗi ngoài dự kiến (I/O, DB, serialization...) → báo fail
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
