using Confluent.Kafka;
using System.Text.Json;

namespace DealerService.Kafka
{
    public class KafkaConsumer<T> : IKafkaConsumer
    {
        private readonly string _topic;
        private readonly string _groupId;
        private readonly ConsumerConfig _config;
        private readonly Func<T, Task> _handle;

        public KafkaConsumer(IConfiguration config, string topicKey, string groupId, Func<T, Task> handle)
        {
            _topic = config[$"KafkaSettings:Topics:{topicKey}"];
            _groupId = groupId;
            _handle = handle;

            _config = new ConsumerConfig
            {
                BootstrapServers = config["KafkaSettings:BootstrapServers"],
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public void StartConsuming(CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe(_topic);

            Console.WriteLine($"[Kafka] Start consuming: {_topic}");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(cancellationToken);
                    var message = JsonSerializer.Deserialize<T>(result.Message.Value);
                    if (message != null)
                        _handle.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Kafka] Consumer stopped");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}