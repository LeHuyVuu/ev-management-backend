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
        private readonly IEnumerable<IOrderEventHandler> _handlers;

        public MultiTopicOrderConsumer(
            ILogger<MultiTopicOrderConsumer> logger,
            IOptions<KafkaSettings> kafkaOptions,
            IEnumerable<IOrderEventHandler> handlers)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions;
            _handlers = handlers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var cfg = _kafkaOptions.Value;
            var topics = _handlers.Select(h => h.Topic).ToList();

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = cfg.BootstrapServers,
                GroupId = "brandservice-multitopic-consumer",
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
                    var handler = _handlers.FirstOrDefault(h => h.Topic == cr.Topic);

                    if (handler != null)
                        await handler.HandleAsync(cr.Message.Value, stoppingToken);
                    else
                        _logger.LogWarning("No handler found for topic {Topic}", cr.Topic);

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
