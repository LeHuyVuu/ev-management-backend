using IdentityService.Infrastructure.Services;
using IdentityService.Kafka.Event;
using IdentityService.Kafka.Messaging.Producer;
using IdentityService.Kafka.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace IdentityService.Kafka.Handler
{
    public class DealerAccountHandler : IDealerAccountEventHandler
    {
        private readonly ILogger<DealerAccountHandler> _logger;
        private readonly IKafkaProducer _producer;
        private readonly KafkaSettings _settings;
        private readonly UserService _userService; // hoặc IDealerAccountService tùy service

        public string Topic => _settings.Topics["DealerAccountStatus"];

        public DealerAccountHandler(
            ILogger<DealerAccountHandler> logger,
            IKafkaProducer producer,
            IOptions<KafkaSettings> kafkaOptions,
            UserService userService)
        {
            _logger = logger;
            _producer = producer;
            _settings = kafkaOptions.Value;
            _userService = userService;
        }

        public async Task HandleAsync(string messageValue, CancellationToken ct)
        {
            var ev = JsonSerializer.Deserialize<DealerAccountStatusEvent>(
                messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ev == null)
            {
                _logger.LogWarning("DealerAccountEvent deserialization failed");
                return;
            }

            _logger.LogInformation("Handling DealerAccountEvent: {@Event}", ev);

            try
            {
                await _userService.UpdateStatusDealerAccount(ev.DealerId, ev.Status);
                _logger.LogInformation("Updated DealerAccount successfully for DealerId={0}", ev.DealerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update DealerAccount for DealerId={0}", ev.DealerId);
            }
        }

    }
}
