using Microsoft.Extensions.Hosting;
using Shared.Kafka;

namespace DealerService.Kafka
{
    public class KafkaConsumerBackgroundService : BackgroundService
    {
        private readonly IKafkaConsumer _consumer;

        public KafkaConsumerBackgroundService(IKafkaConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => _consumer.StartConsuming(stoppingToken), stoppingToken);
        }
    }
}