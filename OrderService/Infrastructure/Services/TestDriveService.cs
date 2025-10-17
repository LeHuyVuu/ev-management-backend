using AutoMapper;
using Microsoft.Extensions.Logging;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Entities;
using OrderService.Infrastructure.Repositories;
using OrderService.Model;

namespace OrderService.Infrastructure.Services
{
    public class TestDriveService
    {
        private readonly ILogger<TestDriveService> _logger;
        private readonly TestDriveRepository _repository;
        private readonly IMapper _mapper;

        public TestDriveService(
            ILogger<TestDriveService> logger,
            TestDriveRepository repository,
            IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
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
                return _mapper.Map<TestDriveResponse>(created);
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
