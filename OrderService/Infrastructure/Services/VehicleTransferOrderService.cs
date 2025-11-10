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

namespace OrderService.Infrastructure.Services
{
    public class VehicleTransferOrderService
    {
        private readonly ILogger<VehicleTransferOrderService> _logger;
        private readonly VehicleTransferOrderRepository _repository;
        private readonly IMapper _mapper;     
        private readonly IKafkaProducer _kafkaProducer;
        private readonly string _kafkaTopic;

        public VehicleTransferOrderService(
            ILogger<VehicleTransferOrderService> logger,
            VehicleTransferOrderRepository repository,
            IMapper mapper,
            IKafkaProducer kafkaProducer,                  
            IOptions<KafkaSettings> kafkaOptions)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
            _kafkaTopic = kafkaOptions.Value.Topics.TryGetValue("Transfer", out var dealerTopic)
                ? dealerTopic
                : throw new InvalidOperationException("KafkaSettings: Dealer topic missing");
        }

        public async Task<PagedResult<VehicleTransferOrderResponse>> GetVehicleTransferOrdersAsync(
            Guid? dealerId, int pageNumber, int pageSize)
        {
            try
            {
                var pagedOrders = await _repository.GetVehicleTransferOrders(dealerId, pageNumber, pageSize);

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

        public async Task<VehicleTransferOrderResponse?> UpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Transfer order not found. Id={Id}", id);
                    return null;
                }

                var response = _mapper.Map<VehicleTransferOrderResponse>(entity);

                var normalized = status?.Trim().ToLowerInvariant();
                var isShipping = normalized == "shipping";
                var isReceived = normalized == "received";

                Guid? dealerForEvent = isShipping
                    ? entity.FromDealerId
                    : isReceived
                        ? entity.ToDealerId
                        : (entity.FromDealerId ?? entity.ToDealerId); 

                if (dealerForEvent is null)
                {
                    _logger.LogWarning(
                        "Publishing TransferOrderEvent without dealer is not allowed. Missing both FromDealerId and ToDealerId. Id={Id}, Status={Status}",
                        id, status);
                    return response; 
                }

                var evt = new TransferOrderEvent
                {
                    OrderId = id,
                    DealerId = dealerForEvent, 
                    VehicleVersionId = entity.VehicleVersionId,
                    Quantity = entity.Quantity,
                    Status = normalized
                };

                var key = dealerForEvent.Value.ToString();

                await _kafkaProducer.PublishAsync(_kafkaTopic, key, evt);

                _logger.LogInformation(
                    "Published TransferOrderEvent. OrderId={OrderId}, Topic={Topic}, DealerId={DealerId}, VehicleVersionId={VehicleVersionId}, Qty={Qty}, Status={EvtStatus}",
                    id, _kafkaTopic, dealerForEvent, entity.VehicleVersionId, entity.Quantity, evt.Status
                );

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle transfer order status. Id={Id}, Status={Status}", id,
                    status);
                throw;
            }
        }
    }
}
