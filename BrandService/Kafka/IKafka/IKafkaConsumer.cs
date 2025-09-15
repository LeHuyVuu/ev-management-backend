namespace DealerService.Kafka
{
    public interface IKafkaConsumer
    {
        void StartConsuming(CancellationToken cancellationToken);
    }
}