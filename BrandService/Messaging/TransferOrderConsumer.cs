using System.Text.Json;
using BrandService.Event;
using Confluent.Kafka;
using BrandService.Infrastructure.Services;
using BrandService.Settings;
using Microsoft.Extensions.Options;

namespace BrandService.Kafka.Messaging
{
    public class TransferOrderConsumer : BackgroundService
    {
        private readonly ILogger<TransferOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly InventoryService _inventoryService;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // camelCase -> PascalCase mapping
        };

        public TransferOrderConsumer(
            ILogger<TransferOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            InventoryService inventoryService)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _inventoryService = inventoryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var kafkaCfg = _kafkaOptions.Value;
            var bootstrap = kafkaCfg.BootstrapServers;
            var topic = kafkaCfg.Topics.TryGetValue("Transfer", out var t)
                ? t
                : throw new InvalidOperationException("KafkaSettings: Transfer topic missing");

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "brandservice-transfer-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation("🚀 Kafka consumer started. Topic={Topic}, GroupId={GroupId}", topic, consumerCfg.GroupId);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);

                    _logger.LogInformation("📥 Received message [Topic={Topic}] [Partition={Partition}] [Offset={Offset}]",
                        cr.Topic, cr.Partition, cr.Offset);

                    try
                    {
                        var ev = JsonSerializer.Deserialize<TransferOrderEvent>(cr.Message.Value, _jsonOpts);

                        if (ev == null)
                        {
                            _logger.LogWarning("⚠️ Deserialized TransferOrderEvent is null. Raw={Raw}", cr.Message.Value);
                        }
                        else
                        {
                            await HandleTransferEventAsync(ev);
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
                _logger.LogInformation("🛑 TransferOrderConsumer stopping...");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed for topic {Topic}", topic);
            }
        }

        private async Task HandleTransferEventAsync(TransferOrderEvent ev)
        {
            try
            {
                if (ev.DealerId == null || ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    _logger.LogWarning("⚠️ Incomplete TransferOrderEvent data: {@Event}", ev);
                    return;
                }

                var delta = ev.IsRecived ? ev.Quantity.Value : -ev.Quantity.Value;

                await _inventoryService.UpdateDealerStockAsync(
                    ev.DealerId.Value,
                    ev.VehicleVersionId.Value,
                    delta
                );

                _logger.LogInformation(
                    "🧾 Updated stock for Dealer={DealerId}, VehicleVersion={VehicleVersionId}, Delta={Delta}, IsReceived={IsReceived}",
                    ev.DealerId, ev.VehicleVersionId, delta, ev.IsRecived
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling TransferOrderEvent {@Event}", ev);
            }
        }
    }
}
