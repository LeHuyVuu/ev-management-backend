using System.Text.Json;
using Confluent.Kafka;

namespace BrandService.Kafka
{
    public class BrandProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        
        

        // Constructor nhận cấu hình từ DI container
        public BrandProducer(IConfiguration configuration)
        {
            var kafkaServer = configuration["KafkaSettings:BootstrapServers"]; // Đọc cấu hình Kafka
            _topic = configuration["KafkaSettings:Topic"]; // Đọc cấu hình topic

            // Cấu hình cho Kafka producer
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaServer
            };

            _producer = new ProducerBuilder<Null, string>(config).Build(); // Tạo instance producer
        }

        // Phương thức gửi message
        public async Task SendMessageAsync(object message)
        {
            try
            {
                // Serialize message thành chuỗi JSON
                var serializedMessage = JsonSerializer.Serialize(message);

                // Gửi thông điệp đến Kafka
                var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = serializedMessage });

                // Log kết quả
                Console.WriteLine($"Message sent to {result.TopicPartitionOffset}");
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                Console.WriteLine($"Error sending message to Kafka: {ex.Message}");
            }
        }
    }
}