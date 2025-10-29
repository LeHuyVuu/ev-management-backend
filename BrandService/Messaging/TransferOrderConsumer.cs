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
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public TransferOrderConsumer(
            ILogger<TransferOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _scopeFactory = scopeFactory;
            
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(" Transfer orrder started.");
            
            try
            {

                var kafkaCfg = _kafkaOptions?.Value;
                if (kafkaCfg == null)
                {
                    _logger.LogError("KafkaSettings is NULL (not bound from config)");
                    return;
                }

                if (kafkaCfg.Topics == null)
                {
                    _logger.LogError("KafkaSettings.Topics is NULL!");
                    return;
                }

                if (!kafkaCfg.Topics.TryGetValue("Transfer", out var topic))
                {
                    _logger.LogError("KafkaSettings does not contain key 'Transfer'");
                    return;
                }


                var consumerCfg = new ConsumerConfig
                {
                    BootstrapServers = kafkaCfg.BootstrapServers,
                    GroupId = "brandservice-transfer-consumer",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
                consumer.Subscribe(topic);


                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    _logger.LogInformation("Received message: {Message}", cr.Message.Value);

                    try
                    {
                        var ev = JsonSerializer.Deserialize<TransferOrderEvent>(cr.Message.Value, _jsonOpts);

                        if (ev == null)
                        {
                            _logger.LogWarning(" Deserialized event is null. Raw={Raw}", cr.Message.Value);
                            continue;
                        }

                        _logger.LogInformation("Deserialized event successfully. Calling handler...");

                        using var scope = _scopeFactory.CreateScope();
                        var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();

                        await HandleTransferEventAsync(ev, inventoryService);

                        consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, " Error processing message: {Message}", cr.Message.Value);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " TransferOrderConsumer failed to start or crashed");
            }
        }


        private async Task HandleTransferEventAsync(TransferOrderEvent ev, InventoryService inventoryService)
        {
                _logger.LogInformation("Transfer order event");
            
            try
            {
                if (ev.DealerId == null || ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    _logger.LogWarning("Incomplete TransferOrderEvent data: {@Event}", ev);
                    return;
                }

                var delta = ev.IsRecived ? ev.Quantity.Value : -ev.Quantity.Value;

                await inventoryService.UpdateDealerStockAsync(
                    ev.DealerId.Value,
                    ev.VehicleVersionId.Value,
                    delta
                );

                _logger.LogInformation(
                    "Updated stock for Dealer={DealerId}, VehicleVersion={VehicleVersionId}, Delta={Delta}, IsReceived={IsReceived}",
                    ev.DealerId, ev.VehicleVersionId, delta, ev.IsRecived
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TransferOrderEvent {@Event}", ev);
            }
        }
    }
}
