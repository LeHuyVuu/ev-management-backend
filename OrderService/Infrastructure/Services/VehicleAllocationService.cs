using AutoMapper;
using Microsoft.Extensions.Options;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Entities;
using OrderService.Infrastructure.Repositories;
using OrderService.Kafka.Events;
using OrderService.Kafka.Messaging.Producer;
using OrderService.Kafka.Settings;
using OrderService.Model;

namespace OrderService.Infrastructure.Services;

public class VehicleAllocationService
{
    private readonly ILogger<VehicleAllocationService> _logger;
    private readonly VehicleAllocationRepository _repository;
    private readonly IMapper _mapper;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly string _kafkaTopic;

    public VehicleAllocationService(
        ILogger<VehicleAllocationService> logger,
        VehicleAllocationRepository repository,
        IMapper mapper,
        IKafkaProducer kafkaProducer,
        IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;

        _kafkaTopic = kafkaOptions.Value.Topics.TryGetValue("Allocation", out var allocationTopic)
            ? allocationTopic
            : throw new InvalidOperationException("KafkaSettings: Allocation topic missing");
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

    public async Task<VehicleAllocationResponse> CreateAsync(VehicleAllocationRequest request, Guid dealerId)
    {
        try
        {
            var entity = _mapper.Map<VehicleAllocation>(request);
            entity.DealerId = dealerId;

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

            var response = _mapper.Map<VehicleAllocationResponse>(updated);

            // ✅ Normalize status
            var normalized = status.Trim().ToLowerInvariant();
            var isShipping = normalized == "shipping";
            var isReceived = normalized == "received";

            if (isShipping || isReceived)
            {
                var ev = new AllocationOrderEvent
                {
                    DealerId = updated.DealerId,
                    VehicleVersionId = updated.VehicleVersionId,
                    Quantity = updated.Quantity,
                    IsRecived = isReceived
                };

                var key = updated.DealerId.ToString();

                await _kafkaProducer.PublishAsync(_kafkaTopic, key, ev);

                _logger.LogInformation(
                    "✅ Published AllocationOrderEvent. AllocationId={Id}, DealerId={DealerId}, VehicleVersionId={VehicleVersionId}, Qty={Qty}, IsReceived={IsReceived}, Topic={Topic}",
                    updated.AllocationId, updated.DealerId, updated.VehicleVersionId, updated.Quantity, ev.IsRecived, _kafkaTopic
                );
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle allocation status for Id={Id}", id);
            throw;
        }
    }
}
