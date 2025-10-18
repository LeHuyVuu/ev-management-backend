using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Entities;
using OrderService.Events;
using OrderService.Infrastructure.Repositories;
using OrderService.Messaging;
using OrderService.Model;
using OrderService.Settings;

namespace OrderService.Infrastructure.Services
{
    public class TestDriveService
    {
        private readonly ILogger<TestDriveService> _logger;
        private readonly TestDriveRepository _repository;
        private readonly IMapper _mapper;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly string _kafkaTopic;
        

        public TestDriveService(
            ILogger<TestDriveService> logger,
            TestDriveRepository repository,
            IMapper mapper,
            IKafkaProducer kafkaProducer,                  // ✅ inject producer
            IOptions<KafkaSettings> kafkaOptions)          // ✅ inject options
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
            _kafkaTopic = kafkaOptions.Value.Topic 
                          ?? throw new InvalidOperationException("KafkaSettings:Topic missing");
        }

        public async Task<PagedResult<TestDriveResponse>> GetTestDrivesAsync(Guid? dealerId, int pageNumber, int pageSize)
        {
            try
            {
                var pagedTestDrives = await _repository.GetTestDrivesAsync(dealerId, pageNumber, pageSize);

                var mapped = new PagedResult<TestDriveResponse>
                {
                    Items = _mapper.Map<List<TestDriveResponse>>(pagedTestDrives.Items),
                    TotalItems = pagedTestDrives.TotalItems,
                    PageNumber = pagedTestDrives.PageNumber,
                    PageSize = pagedTestDrives.PageSize
                };

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving test drives.");
                throw;
            }
        }

        public async Task<TestDriveResponse> GetByIdAsync(Guid id)
        {
            try
            {
                var testDrive = await _repository.GetByIdAsync(id);
                return _mapper.Map<TestDriveResponse>(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving test drive with ID {Id}.", id);
                throw;
            }
        }

        public async Task<TestDriveResponse> CreateAsync(TestDriveRequest request, Guid dealerId)
        {
            try
            {
                var entity = _mapper.Map<TestDrife>(request);
                entity.DealerId = dealerId;

                var created = await _repository.CreateAsync(entity);
                if (created is null)
                {
                    _logger.LogError("Repository.CreateAsync returned null for dealer {DealerId}", dealerId);
                    return null!;
                }

                var response = _mapper.Map<TestDriveResponse>(created);
                if (response is null)
                {
                    _logger.LogError("AutoMapper mapped null for entity {Entity}", created);
                    return null!;
                }

                var testDriveId = created.GetType().GetProperty("TestDriveId") != null
                    ? (Guid)created.GetType().GetProperty("TestDriveId")!.GetValue(created)!
                    : (Guid)created.GetType().GetProperty("Id")!.GetValue(created)!;

                var ev = new TestDriveCreatedEvent(
                    TestDriveId: testDriveId,
                    DealerId: dealerId,
                    CustomerId: request.CustomerId,
                    VehicleVersionId: request.VehicleVersionId,
                    DriveDate: request.DriveDate,
                    TimeSlot: request.TimeSlot ?? "",
                    ConfirmSms: request.ConfirmSms,
                    ConfirmEmail: request.ConfirmEmail,
                    Status: request.Status ?? "",
                    OccurredAtUtc: DateTime.UtcNow
                );

                // ✅ dùng topic đã lấy sẵn từ options
                await _kafkaProducer.PublishAsync(_kafkaTopic, dealerId.ToString(), ev);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating test drive for dealer {DealerId}.", dealerId);
                throw;
            }
        }
    



        public async Task<TestDriveResponse?> UpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var updated = await _repository.UpdateStatusAsync(id, status);
                if (updated == null)
                    return null;

                return _mapper.Map<TestDriveResponse>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating test drive status for ID {Id}.", id);
                throw;
            }
        }
    }
}
