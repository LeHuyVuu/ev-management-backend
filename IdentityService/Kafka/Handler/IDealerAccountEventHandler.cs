namespace IdentityService.Kafka.Handler
{
    public interface IDealerAccountEventHandler
    {
        string Topic { get; }
        Task HandleAsync(string messageValue, CancellationToken ct);
    }
}
