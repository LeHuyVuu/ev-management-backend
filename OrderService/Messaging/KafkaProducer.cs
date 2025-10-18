using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderService.Settings;
using System.Text.Json;

namespace OrderService.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly string _bootstrap;
    public KafkaProducer(IOptions<KafkaSettings> opts)
    {
        _bootstrap = opts.Value.BootstrapServers 
                     ?? throw new InvalidOperationException("KafkaSettings:BootstrapServers missing");
    }

    public async Task PublishAsync<T>(string topic, string key, T payload, CancellationToken ct = default)
    {
        var cfg = new ProducerConfig { BootstrapServers = _bootstrap };
        using var producer = new ProducerBuilder<string, string>(cfg).Build();

        var json = JsonSerializer.Serialize(payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var result = await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json }, ct);
        if (result.Status is not PersistenceStatus.Persisted)
            throw new Exception($"Kafka not persisted: {result.Status}");
    }
}