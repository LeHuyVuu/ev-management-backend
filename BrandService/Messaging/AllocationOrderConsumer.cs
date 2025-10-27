using System.Text.Json;
using Confluent.Kafka;
using BrandService.Infrastructure.Services;
using BrandService.Settings;
using Microsoft.Extensions.Options;
using BrandService.Event; // AllocationOrderEvent

namespace BrandService.Kafka.Messaging
{
    public class AllocationOrderConsumer : BackgroundService
    {
        private readonly ILogger<AllocationOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly BrandInventoryService _brandInventoryService;
        private readonly InventoryService _inventoryService; // ✅ để gọi cộng kho

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AllocationOrderConsumer(
            ILogger<AllocationOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            BrandInventoryService brandInventoryService,
            InventoryService inventoryService)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _brandInventoryService = brandInventoryService;
            _inventoryService = inventoryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var kafkaCfg = _kafkaOptions.Value;
            var bootstrap = kafkaCfg.BootstrapServers;

            var topic = kafkaCfg.Topics.TryGetValue("Allocation", out var t)
                ? t
                : throw new InvalidOperationException("KafkaSettings: Allocation topic missing");

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "brandservice-allocation-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation("🚀 AllocationOrderConsumer started. Topic={Topic}, GroupId={GroupId}", topic, consumerCfg.GroupId);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);

                    _logger.LogInformation("📥 Received message [Topic={Topic}] [Partition={Partition}] [Offset={Offset}]",
                        cr.Topic, cr.Partition, cr.Offset);

                    try
                    {
                        var ev = JsonSerializer.Deserialize<AllocationOrderEvent>(cr.Message.Value, _jsonOpts);

                        if (ev == null)
                        {
                            _logger.LogWarning("⚠️ Deserialized AllocationOrderEvent is null. Raw={Raw}", cr.Message.Value);
                        }
                        else
                        {
                            await HandleAllocationEventAsync(ev);
                        }

                        consumer.Commit(cr);
                        _logger.LogInformation("✅ Committed offset {Offset}", cr.Offset);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing message at {TopicPartitionOffset}", cr.TopicPartitionOffset);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 AllocationOrderConsumer stopping...");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed for topic {Topic}", topic);
            }
        }

        private async Task HandleAllocationEventAsync(AllocationOrderEvent ev)
        {
            try
            {
                if (ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    _logger.LogWarning("⚠️ Incomplete AllocationOrderEvent data: {@Event}", ev);
                    return;
                }

                // 👉 Nếu IsRecived = false => trừ kho Brand
                if (!ev.IsRecived)
                {
                    var delta = -ev.Quantity.Value;

                    await _brandInventoryService.UpdateStockAsync(ev.VehicleVersionId.Value, delta);

                    _logger.LogInformation(
                        "🏭 Brand stock updated (shipping). VehicleVersion={VehicleVersionId}, Delta={Delta}",
                        ev.VehicleVersionId, delta
                    );
                }
                else
                {
                    // 👉 Nếu IsRecived = true => cộng kho đại lý
                    if (ev.DealerId != null)
                    {
                        await _inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId!.Value, ev.Quantity!.Value);

                        _logger.LogInformation(
                            "🏪 Dealer stock updated (received). DealerId={DealerId}, VehicleVersion={VehicleVersionId}, +{Qty}",
                            ev.DealerId, ev.VehicleVersionId, ev.Quantity
                        );
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Allocation event missing DealerId on IsRecived=true. {@Event}", ev);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling AllocationOrderEvent {@Event}", ev);
            }
        }
    }
}
