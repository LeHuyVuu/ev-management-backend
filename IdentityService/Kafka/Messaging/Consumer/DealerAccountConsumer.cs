using Confluent.Kafka;
using IdentityService.Kafka.Handler;
using IdentityService.Kafka.Settings;
using Microsoft.Extensions.Options;

namespace IdentityService.Kafka.Messaging.Consumer
{
    public class DealerAccountConsumer : BackgroundService
    {
        private readonly ILogger<DealerAccountConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly IServiceScopeFactory _scopeFactory;

        public DealerAccountConsumer(
            ILogger<DealerAccountConsumer> logger,
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
            if (!cfg.Topics.TryGetValue("DealerAccountStatus", out var topic))
                throw new InvalidOperationException("KafkaSettings.Topics['DealerAccountStatus'] missing.");

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = cfg.BootstrapServers,
                GroupId = "user-dealeraccount-consumer", // đặt tên group cho UserService
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerCfg).Build();
            consumer.Subscribe(topic);
            _logger.LogInformation("UserService subscribed to topic: {Topic}", topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    // Lấy đúng handler của UserService (đã đăng ký DI)
                    var handler = scope.ServiceProvider.GetRequiredService<IDealerAccountEventHandler>();

                    try
                    {
                        await handler.HandleAsync(cr.Message.Value, stoppingToken);
                        consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "UserService failed handling {TPO}", cr.TopicPartitionOffset);
                        // tuỳ chiến lược retry/DLQ, có thể bỏ Commit để retry
                        consumer.Commit(cr);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UserService consumer stopping...");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("UserService consumer closed.");
            }
        }
    }
}
