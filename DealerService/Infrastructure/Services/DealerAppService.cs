using AutoMapper;
using DealerService.DTOs.Requests.DealerDTOs;
using DealerService.DTOs.Responses.DealerDTOs;
using DealerService.Entities;
using DealerService.Infrastructure.Repositories;
using DealerService.Kafka.Event;
using DealerService.Kafka.Messaging.Producer;
using DealerService.Kafka.Settings;
using DealerService.Models;
using Microsoft.Extensions.Options;

namespace DealerService.Infrastructure.Services
{
    public class DealerAppService
    {
        private readonly DealerRepository _dealerRepo;
        private readonly IMapper _mapper;
        private readonly IKafkaProducer _producer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<DealerAppService> _logger;

        public DealerAppService(
            DealerRepository dealerRepo,
            IMapper mapper,
            IKafkaProducer producer,
            IOptions<KafkaSettings> settings,
            ILogger<DealerAppService> logger)
        {
            _dealerRepo = dealerRepo;
            _mapper = mapper;
            _producer = producer;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<PagedResult<DealerResponse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var paged = await _dealerRepo.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<DealerResponse>
            {
                Items = _mapper.Map<List<DealerResponse>>(paged.Items),
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };
        }

        public async Task<List<DealerResponse>> GetAllActiveDealersAsync()
        {
            var dealers = await _dealerRepo.GetAllActiveDealersAsync();
            return _mapper.Map<List<DealerResponse>>(dealers);
        }

        public async Task<DealerResponse?> GetByIdAsync(Guid id)
        {
            var dealer = await _dealerRepo.GetByIdAsync(id);
            return _mapper.Map<DealerResponse>(dealer);
        }

        public async Task<DealerResponse> CreateAsync(DealerRequest dealerRequest)
        {
            var dealer = _mapper.Map<Dealer>(dealerRequest);
            dealer.DealerId = Guid.NewGuid();
            var created = await _dealerRepo.AddAsync(dealer);
            return _mapper.Map<DealerResponse>(created);
        }

        public async Task<DealerResponse?> UpdateAsync(Guid id, DealerRequest dealerRequest)
        {
            var dealer = await _dealerRepo.GetByIdAsync(id);
            if (dealer == null)
            {
                _logger.LogWarning("Dealer {DealerId} not found", id);
                return null;
            }

            // Map fields từ request sang entity
            dealer = _mapper.Map(dealerRequest, dealer);
            var updated = await _dealerRepo.UpdateAsync(dealer);
            var response = _mapper.Map<DealerResponse>(updated);

            // 🔹 Kiểm tra status khác "active" thì gửi Kafka event
            if (!string.Equals(dealer.Status, updated.Status, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(updated.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                if (!_settings.Topics.TryGetValue("DealerAccountStatus", out var topic))
                    throw new InvalidOperationException("KafkaSettings.Topics['DealerAccountStatus'] missing.");

                var ev = new DealerAccountStatusEvent
                {
                    DealerId = updated.DealerId,
                    Status = updated.Status
                };

                await _producer.PublishAsync(topic, updated.DealerId.ToString(), ev);
                _logger.LogInformation("Published DealerAccountStatusEvent for DealerId={DealerId}, Status={Status}", updated.DealerId, updated.Status);
            }

            return response;
        }
    }
}
