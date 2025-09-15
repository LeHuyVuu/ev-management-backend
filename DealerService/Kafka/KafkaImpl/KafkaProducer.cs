using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;


namespace Shared.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T>
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;


        public KafkaProducer(IConfiguration config, string topicConfigKey)
        {
            var bootstrapServers = config["KafkaSettings:BootstrapServers"]
                                   ?? throw new ArgumentNullException("KafkaSettings:BootstrapServers");


            _topic = config[$"KafkaSettings:Topics:{topicConfigKey}"]
                     ?? throw new ArgumentNullException($"KafkaSettings:Topics:{topicConfigKey}");


            var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }


        public async Task SendAsync(T message)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(message);
                var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = serialized });
                Console.WriteLine($"[Kafka] Message sent to {_topic} at {result.TopicPartitionOffset}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Kafka] Send failed: {ex.Message}");
            }
        }
    }
}