namespace BrandService.Kafka.Handler;

public interface IOrderEventHandler
{
    string Topic { get; }
    Task HandleAsync(string messageValue, CancellationToken ct);
}