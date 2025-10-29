using System.Text.Json;
using Confluent.Kafka;
using BrandService.Event;
using BrandService.Infrastructure.Services;
using BrandService.Settings;
using Microsoft.Extensions.Options;

namespace BrandService.Kafka.Messaging
{
    public class MultiTopicOrderConsumer : BackgroundService
    {
        private readonly ILogger<MultiTopicOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public MultiTopicOrderConsumer(
            ILogger<MultiTopicOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var kafkaCfg = _kafkaOptions.Value;
            if (kafkaCfg == null)
            {
                _logger.LogError("KafkaSettings is NULL — check appsettings.");
                return;
            }

            var bootstrap = kafkaCfg.BootstrapServers;
            if (kafkaCfg.Topics == null)
            {
                _logger.LogError("KafkaSettings.Topics is NULL — missing in config.");
                return;
            }

            if (!kafkaCfg.Topics.TryGetValue("Transfer", out var transferTopic) ||
                !kafkaCfg.Topics.TryGetValue("Allocation", out var allocationTopic))
            {
                _logger.LogError("Missing Transfer or Allocation topic in KafkaSettings.");
                return;
            }

            var topics = new[] { transferTopic, allocationTopic };

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "brandservice-multitopic-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
            consumer.Subscribe(topics);

            _logger.LogInformation("MultiTopicOrderConsumer started. Topics = {Topics}", string.Join(", ", topics));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    _logger.LogInformation("Received message from {Topic}: {Value}", cr.Topic, cr.Message.Value);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        if (cr.Topic == transferTopic)
                        {
                            var ev = JsonSerializer.Deserialize<TransferOrderEvent>(cr.Message.Value, _jsonOpts);
                            if (ev != null)
                            {
                                var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
                                await HandleTransferEventAsync(ev, inventoryService);
                            }
                        }
                        else if (cr.Topic == allocationTopic)
                        {
                            var ev = JsonSerializer.Deserialize<AllocationOrderEvent>(cr.Message.Value, _jsonOpts);
                            if (ev != null)
                            {
                                var brandInventoryService = scope.ServiceProvider.GetRequiredService<BrandInventoryService>();
                                var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
                                await HandleAllocationEventAsync(ev, brandInventoryService, inventoryService);
                            }
                        }

                        consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from {Topic}", cr.Topic);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MultiTopicOrderConsumer stopping...");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }

        private async Task HandleTransferEventAsync(TransferOrderEvent ev, InventoryService inventoryService)
        {
            try
            {
                _logger.LogInformation("Handling TransferOrderEvent...");
                if (ev.DealerId == null || ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    _logger.LogWarning("Incomplete TransferOrderEvent data: {@Event}", ev);
                    return;
                }

                var delta = ev.IsRecived ? ev.Quantity.Value : -ev.Quantity.Value;
                await inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId.Value, delta);

                _logger.LogInformation(
                    "Updated dealer stock. Dealer={DealerId}, VehicleVersion={Version}, Delta={Delta}, IsReceived={IsReceived}",
                    ev.DealerId, ev.VehicleVersionId, delta, ev.IsRecived);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TransferOrderEvent {@Event}", ev);
            }
        }

        private async Task HandleAllocationEventAsync(
            AllocationOrderEvent ev,
            BrandInventoryService brandInventoryService,
            InventoryService inventoryService)
        {
            try
            {
                _logger.LogInformation("Handling AllocationOrderEvent...");

                if (ev.VehicleVersionId == null || ev.Quantity == null)
                {
                    _logger.LogWarning("Incomplete AllocationOrderEvent data: {@Event}", ev);
                    return;
                }

                if (!ev.IsRecived)
                {
                    await brandInventoryService.UpdateStockAsync(ev.VehicleVersionId.Value, -ev.Quantity.Value);
                    _logger.LogInformation("Brand stock -{Qty}", ev.Quantity);
                }
                else if (ev.DealerId != null)
                {
                    await inventoryService.UpdateDealerStockAsync(ev.DealerId.Value, ev.VehicleVersionId.Value, ev.Quantity.Value);
                    _logger.LogInformation("Dealer stock +{Qty}", ev.Quantity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling AllocationOrderEvent {@Event}", ev);
            }
        }
    }
}
