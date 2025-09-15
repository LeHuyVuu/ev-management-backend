using System.Threading;


namespace Shared.Kafka
{
    public interface IKafkaConsumer
    {
        void StartConsuming(CancellationToken cancellationToken);
    }
}