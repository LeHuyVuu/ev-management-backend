// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using Confluent.Kafka;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using ProductService.Model;
//
//
// namespace ProductService.Kafka
// {
//
//     public class ProductStockUpdateConsumer : BackgroundService
//     {
//         private readonly IServiceScopeFactory _serviceScopeFactory;
//         private readonly IConfiguration _configuration;
//         private readonly ILogger<ProductStockUpdateConsumer> _logger;
//
//    
//         public ProductStockUpdateConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration,
//             ILogger<ProductStockUpdateConsumer> logger)
//         {
//             _serviceScopeFactory = serviceScopeFactory;
//             _configuration = configuration;
//             _logger = logger;
//         }
//
//
//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             var kafkaServer = _configuration["KafkaSettings:BootstrapServers"];
//             var topic = _configuration["KafkaSettings:Topic"];
//
//             var config = new ConsumerConfig
//             {
//                 GroupId = "product-stock-consumer-group",
//                 BootstrapServers = kafkaServer,
//                 AutoOffsetReset = AutoOffsetReset.Earliest,
//                 EnableAutoCommit = false
//             };
//
//             using var consumer = new ConsumerBuilder<string, string>(config).Build();
//             consumer.Subscribe(topic);
//
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     _logger.LogInformation("Waiting for messages...");
//
//                     var consumeResult = consumer.Consume(stoppingToken);
//
//                     _logger.LogInformation($"Received message: {consumeResult.Message.Value}");
//
//                     var stockUpdateMessage =
//                         JsonSerializer.Deserialize<ProductStockUpdateMessage>(consumeResult.Message.Value);
//                     if (stockUpdateMessage != null)
//                     {
//                         _logger.LogInformation($"Processing stock update for ProductId {stockUpdateMessage.ProductId}");
//                         await HandleStockUpdate(stockUpdateMessage);
//                     }
//                     else
//                     {
//                         _logger.LogWarning("Failed to deserialize message.");
//                     }
//
//                     consumer.Commit(consumeResult);
//                     _logger.LogInformation($"Committed offset: {consumeResult.TopicPartitionOffset}");
//                 }
//                 catch (ConsumeException ex)
//                 {
//                     _logger.LogError($"Error while consuming: {ex.Message}");
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError($"Unexpected error: {ex.Message}");
//                 }
//             }
//         }
//
//
//         private async Task HandleStockUpdate(ProductStockUpdateMessage message)
//         {
//             try
//             {
//                 if (message.QuantityUpdated <= 0)
//                 {
//                     _logger.LogWarning(
//                         $"Invalid quantity update: {message.QuantityUpdated} for ProductId {message.ProductId}");
//                     return;
//                 }
//
//                 using (var scope = _serviceScopeFactory.CreateScope())
//                 {
//                     var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                     var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
//
//                     await unitOfWork.BeginTransactionAsync(); // 🔁 Bắt đầu transaction
//
//                     var product = await productRepository.GetByIdAsync(message.ProductId);
//                     if (product == null)
//                     {
//                         _logger.LogError($"Product not found for ProductId {message.ProductId}");
//                         await unitOfWork.RollbackTransactionAsync(); // 🔁 rollback nếu không tìm thấy
//                         return;
//                     }
//
//                     if (product.stock_in_quantity < message.QuantityUpdated)
//                     {
//                         _logger.LogWarning(
//                             $"Insufficient stock for ProductId {message.ProductId}. Available: {product.stock_in_quantity}, Needed: {message.QuantityUpdated}");
//                         await unitOfWork.RollbackTransactionAsync(); // 🔁 rollback nếu không đủ hàng
//                         return;
//                     }
//
//                     // ✅ Update tồn kho và số lượng đã bán
//                     product.stock_in_quantity -= message.QuantityUpdated;
//                     product.sold_quantity += message.QuantityUpdated;
//
//                     unitOfWork.ProductRepository.Update(product);
//                     await unitOfWork.SaveAsync();
//
//                     await unitOfWork.CommitTransactionAsync(); // ✅ commit nếu thành công
//
//                     _logger.LogInformation(
//                         $"Stock updated successfully for ProductId {message.ProductId}. New stock: {product.stock_in_quantity}");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, $"Exception when updating stock for ProductId {message.ProductId}");
//
//                 // 🔁 rollback trong catch nếu transaction đã được bắt đầu
//                 try
//                 {
//                     using (var scope = _serviceScopeFactory.CreateScope())
//                     {
//                         var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//                         await unitOfWork.RollbackTransactionAsync();
//                     }
//                 }
//                 catch (Exception rollbackEx)
//                 {
//                     _logger.LogError(rollbackEx, "Rollback failed.");
//                 }
//             }
//         }
//     }
// }