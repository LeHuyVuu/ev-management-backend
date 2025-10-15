using AutoMapper;
using IntelliAIService.DTOs.Requests;
using IntelliAIService.DTOs.Responses;
using IntelliAIService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using OrderService.Entities;
using OrderService.Model;

namespace IntelliAIService.Infrastructure.Services
{
    public class VehicleTransferOrderService
    {
        private readonly ILogger<VehicleTransferOrderService> _logger;
        private readonly VehicleTransferOrderRepository _repository;
        private readonly IMapper _mapper;

        public VehicleTransferOrderService(
            ILogger<VehicleTransferOrderService> logger,
            VehicleTransferOrderRepository repository,
            IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        // ✅ 1️⃣ Get paged list of transfer orders
        public async Task<PagedResult<VehicleTransferOrderResponse>> GetVehicleTransferOrdersAsync(
            Guid? fromDealerId, Guid? toDealerId, int pageNumber, int pageSize)
        {
            try
            {
                var pagedOrders = await _repository.GetVehicleTransferOrders(fromDealerId, toDealerId, pageNumber, pageSize);

                var mapped = new PagedResult<VehicleTransferOrderResponse>
                {
                    Items = _mapper.Map<List<VehicleTransferOrderResponse>>(pagedOrders.Items),
                    TotalItems = pagedOrders.TotalItems,
                    PageNumber = pagedOrders.PageNumber,
                    PageSize = pagedOrders.PageSize
                };

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle transfer orders");
                throw;
            }
        }

        // ✅ 2️⃣ Create new transfer order
        public async Task<VehicleTransferOrderResponse> CreateAsync(VehicleTransferOrderRequest request)
        {
            try
            {
                var entity = _mapper.Map<VehicleTransferOrder>(request);

                var created = await _repository.CreateAsync(entity);

                return _mapper.Map<VehicleTransferOrderResponse>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle transfer order");
                throw;
            }
        }

        // ✅ 3️⃣ Update status of transfer order
        public async Task<VehicleTransferOrderResponse?> UpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var updated = await _repository.UpdateStatusAsync(id, status);
                if (updated == null)
                    return null;

                return _mapper.Map<VehicleTransferOrderResponse>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle transfer order status");
                throw;
            }
        }
    }
}
