using BrandService.Kafka.Handler;
using BrandService.Kafka.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace BrandService.Kafka.Messaging.Consumer
{
    public class MultiTopicOrderConsumer : BackgroundService
    {
        private readonly ILogger<MultiTopicOrderConsumer> _logger;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly IServiceScopeFactory _scopeFactory;

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

            using (var initScope = _scopeFactory.CreateScope())
            {
                var initHandlers = initScope.ServiceProvider.GetRequiredService<IEnumerable<IOrderEventHandler>>();
                var topics = initHandlers.Select(h => h.Topic).Distinct().ToList();

                var cfg = _kafkaOptions.Value;

                var consumerCfg = new ConsumerConfig
                {
                    BootstrapServers = cfg.BootstrapServers,
                    GroupId = "brandservice-multitopic-consumer-v3",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
                consumer.Subscribe(topics);
                _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", topics));

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(stoppingToken);

                        using var msgScope = _scopeFactory.CreateScope();
                        var handlers = msgScope.ServiceProvider.GetRequiredService<IEnumerable<IOrderEventHandler>>();
                        var handler = handlers.FirstOrDefault(h => h.Topic == cr.Topic);

                        if (handler != null)
                        {
                            _logger.LogInformation("Run handler");
                            await handler.HandleAsync(cr.Message.Value, stoppingToken);
                        }
                        else
                        {
                            _logger.LogWarning("No handler found for topic {Topic}", cr.Topic);
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
}
