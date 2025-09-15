using System.Threading.Tasks;


namespace Shared.Kafka
{
    public interface IKafkaProducer<T>
    {
        Task SendAsync(T message);
    }
}