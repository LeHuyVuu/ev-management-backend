namespace OrderService.Messaging;


public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, string key, T payload, CancellationToken ct = default);
}