using System.Text.Json;
using Confluent.Kafka;
using BrandService.Infrastructure.Services;
using BrandService.Settings;
using Microsoft.Extensions.Options;
using BrandService.Event;

namespace BrandService.Kafka.Messaging
{
    public class AllocationOrderConsumer : BackgroundService
    {
        private readonly ILogger<AllocationOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly IServiceScopeFactory _scopeFactory; // ✅ Thay vì inject service trực tiếp

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AllocationOrderConsumer(
            ILogger<AllocationOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            IServiceScopeFactory scopeFactory) // ✅ chỉ inject scope factory
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var kafkaCfg = _kafkaOptions.Value;
            var bootstrap = kafkaCfg.BootstrapServers;
            var topic = kafkaCfg.Topics["Allocation"];

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "brandservice-allocation-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation(" AllocationOrderConsumer started. Topic={Topic}", topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);

                    try
                    {
                        var ev = JsonSerializer.Deserialize<AllocationOrderEvent>(cr.Message.Value, _jsonOpts);
                        if (ev != null)
                        {
                            // ✅ Tạo scope mới để dùng Scoped service
                            using var scope = _scopeFactory.CreateScope();
                            var brandInventoryService = scope.ServiceProvider.GetRequiredService<BrandInventoryService>();
                            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();

                            await HandleEventAsync(ev, brandInventoryService, inventoryService);
                        }

                        consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(" Consumer stopping...");
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task HandleEventAsync(
            AllocationOrderEvent ev,
            BrandInventoryService brandInventoryService,
            InventoryService inventoryService)
        {
            if (ev.VehicleVersionId == null || ev.Quantity == null)
                return;

            if (!ev.IsRecived)
            {
                // Trừ kho hãng
                await brandInventoryService.UpdateStockAsync(ev.VehicleVersionId.Value, -ev.Quantity.Value);
                _logger.LogInformation("🏭 Brand stock -{Qty}", ev.Quantity);
            }
            else if (ev.DealerId != null)
            {
                // Cộng kho dealer
                await inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId.Value, ev.Quantity.Value);
                _logger.LogInformation("🏪 Dealer stock +{Qty}", ev.Quantity);
            }
        }
    }
}
