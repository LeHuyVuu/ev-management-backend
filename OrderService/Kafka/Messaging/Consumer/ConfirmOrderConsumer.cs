using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.Infrastructure.Repositories;
using OrderService.Kafka.Events;
using OrderService.Kafka.Settings;

namespace OrderService.Kafka.Messaging.Consumer
{
    public class ConfirmOrderConsumer : BackgroundService
    {
        private readonly ILogger<ConfirmOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly IServiceScopeFactory _scopeFactory; 

        public ConfirmOrderConsumer(
            ILogger<ConfirmOrderConsumer> logger,
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

            var cfg = _kafkaOptions.Value;

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = cfg.BootstrapServers,
                GroupId = "orderservice-confirm-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
            consumer.Subscribe("confirm-topic");
            _logger.LogInformation("Subscribed to topic: confirm-topic");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value == null) continue;

                    ConfirmOrderEvent? ev;
                    try
                    {
                        ev = JsonSerializer.Deserialize<ConfirmOrderEvent>(
                            cr.Message.Value,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize ConfirmOrderEvent: {Payload}", cr.Message.Value);
                        // TODO: cân nhắc đẩy sang dead-letter-topic thay vì commit
                        consumer.Commit(cr);
                        continue;
                    }

                    if (ev == null || ev.OrderId == null)
                    {
                        _logger.LogWarning("Invalid ConfirmOrderEvent received: {Payload}", cr.Message.Value);
                        consumer.Commit(cr);
                        continue;
                    }

                    _logger.LogInformation(
                        "Processing ConfirmOrderEvent for OrderId={OrderId}, Type={OrderType}, Approved={IsApproved}",
                        ev.OrderId, ev.OrderType, ev.IsApproved);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var allocationRepo = scope.ServiceProvider.GetRequiredService<VehicleAllocationRepository>();
                        var transferRepo   = scope.ServiceProvider.GetRequiredService<VehicleTransferOrderRepository>();

                        string newStatus = !ev.IsApproved ? "Rejected" : (ev.Status ?? "Approved");

                        if (ev.OrderType?.ToLower() == "allocation")
                        {
                            var result = await allocationRepo.UpdateStatusAsync(ev.OrderId, newStatus);
                            if (result == null)
                                _logger.LogWarning("AllocationOrder not found: {OrderId}", ev.OrderId);
                            else
                                _logger.LogInformation("Updated AllocationOrder {OrderId} -> {Status}", ev.OrderId, newStatus);
                        }
                        else if (ev.OrderType?.ToLower() == "transfer")
                        {
                            var result = await transferRepo.UpdateStatusAsync(ev.OrderId, newStatus);
                            if (result == null)
                                _logger.LogWarning("TransferOrder not found: {OrderId}", ev.OrderId);
                            else
                                _logger.LogInformation("Updated TransferOrder {OrderId} -> {Status}", ev.OrderId, newStatus);
                        }
                        else
                        {
                            _logger.LogWarning("Unknown order type {OrderType} in ConfirmOrderEvent", ev.OrderType);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating order status for {OrderId}", ev.OrderId);
                    }

                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping...");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }
    }
}
