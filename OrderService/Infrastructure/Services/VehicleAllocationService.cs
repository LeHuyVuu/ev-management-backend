using AutoMapper;
using IntelliAIService.DTOs.Requests;
using IntelliAIService.DTOs.Responses;
using IntelliAIService.Infrastructure.Repositories;
using OrderService.Entities;
using OrderService.Model;

namespace IntelliAIService.Infrastructure.Services;

public class VehicleAllocationService
{
    private readonly ILogger<VehicleAllocationService> _logger;
    private readonly VehicleAllocationRepository _repository;
    private readonly IMapper _mapper;

    public VehicleAllocationService(
        ILogger<VehicleAllocationService> logger,
        VehicleAllocationRepository repository,
        IMapper mapper)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<VehicleAllocationResponse>> GetVehicleAllocationsAsync(Guid? dealerId, int pageNumber, int pageSize)
    {
        try
        {
            var pagedAllocations = await _repository.GetVehicleAllocations(dealerId, pageNumber, pageSize);

            var mapped = new PagedResult<VehicleAllocationResponse>
            {
                Items = _mapper.Map<List<VehicleAllocationResponse>>(pagedAllocations.Items),
                TotalItems = pagedAllocations.TotalItems,
                PageNumber = pagedAllocations.PageNumber,
                PageSize = pagedAllocations.PageSize
            };

            return mapped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle allocations");
            throw;
        }
    }

    public async Task<VehicleAllocationResponse> CreateAsync(VehicleAllocationRequest request)
    {
        try
        {
            var entity = _mapper.Map<VehicleAllocation>(request);

            var created = await _repository.CreateAsync(entity);

            return _mapper.Map<VehicleAllocationResponse>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle allocation");
            throw;
        }
    }

    public async Task<VehicleAllocationResponse?> UpdateStatusAsync(Guid id, string status)
    {
        try
        {
            var updated = await _repository.UpdateStatusAsync(id, status);
            if (updated == null)
                return null;

            return _mapper.Map<VehicleAllocationResponse>(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle allocation status");
            throw;
        }
    }
}
