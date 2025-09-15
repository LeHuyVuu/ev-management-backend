using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Shared.Kafka;

public class KafkaConsumer<T> : IKafkaConsumer
{
    private readonly string _topic;
    private readonly string _groupId;
    private readonly ConsumerConfig _config;
    private readonly Func<T, Task> _handleMessage;


    public KafkaConsumer(IConfiguration configuration, string topicConfigKey, string groupId,
        Func<T, Task> handleMessage)
    {
        _topic = configuration[$"KafkaSettings:Topics:{topicConfigKey}"]
                 ?? throw new ArgumentNullException($"KafkaSettings:Topics:{topicConfigKey}");


        _groupId = groupId;
        _handleMessage = handleMessage;


        _config = new ConsumerConfig
        {
            BootstrapServers = configuration["KafkaSettings:BootstrapServers"],
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }


    public void StartConsuming(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        consumer.Subscribe(_topic);


        Console.WriteLine($"[Kafka] Started consuming topic: {_topic}");


        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var cr = consumer.Consume(cancellationToken);
                var msg = JsonSerializer.Deserialize<T>(cr.Message.Value);
                _handleMessage?.Invoke(msg!);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[Kafka] Consumer cancelled");
        }
        finally
        {
            consumer.Close();
        }
    }
}