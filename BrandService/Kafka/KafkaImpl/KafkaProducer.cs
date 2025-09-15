using Confluent.Kafka;
using System.Text.Json;
using Shared.Kafka;

namespace DealerService.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T>
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducer(IConfiguration config, string topicKey)
        {
            var bootstrapServers = config["KafkaSettings:BootstrapServers"]
                                   ?? throw new ArgumentNullException("KafkaSettings:BootstrapServers");

            _topic = config[$"KafkaSettings:Topics:{topicKey}"]
                     ?? throw new ArgumentNullException($"KafkaSettings:Topics:{topicKey}");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task SendAsync(T message)
        {
            var value = JsonSerializer.Serialize(message);
            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = value });
            Console.WriteLine($"[Kafka] Sent message to {_topic}: {value}");
        }
    }
}