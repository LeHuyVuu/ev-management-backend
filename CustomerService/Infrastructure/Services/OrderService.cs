using AutoMapper;
using CustomerService.DTOs.Requests.OrderDTOs;
using CustomerService.DTOs.Responses.OrderDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;
using CustomerService.Models;

namespace CustomerService.Infrastructure.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly ContractRepository _contractRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly QuoteRepository _quoteRepository;
    private readonly IMapper _mapper;

    public OrderService(
        OrderRepository orderRepository, 
        CustomerRepository customerRepository,
        ContractRepository contractRepository,
        DealerRepository dealerRepository,
        QuoteRepository quoteRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _contractRepository = contractRepository;
        _dealerRepository = dealerRepository;
        _quoteRepository = quoteRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderCustomerResponse>> GetAllOrdersByCustomerId(Guid customerId)
    {
        var customerExists = await _customerRepository.CustomerExists(customerId);
        if (!customerExists)
            throw new KeyNotFoundException("Customer does not exist");

        var orders = await _orderRepository.GetAllOrdersByCustomerId(customerId);
        if (orders == null || !orders.Any())
            return Enumerable.Empty<OrderCustomerResponse>();

        var orderResponses = _mapper.Map<IEnumerable<OrderCustomerResponse>>(orders) 
                             ?? Enumerable.Empty<OrderCustomerResponse>();

        return orderResponses;
    }

    public async Task<bool> CreateOrder(Guid dealerId, OrderCreateRequest request)
    {
        if(request.ContractId == Guid.Empty)
            throw new KeyNotFoundException("Please input contract Id");
        if(request.CustomerId == Guid.Empty)
            throw new KeyNotFoundException("Please input customer Id");
        
        var customer = await _customerRepository.GetCustomerById(request.CustomerId);
        if(customer == null)
            throw new KeyNotFoundException("Customer does not exist");
        var contract = await _contractRepository.GetContractByContractId(request.ContractId);
        if(contract == null)
            throw new KeyNotFoundException("Contract does not exist");
        
        if(request.DeliveryAddress == null)
            throw new KeyNotFoundException("Delivery Address is not null");
        if(request.DeliveryDate == null)
            throw new KeyNotFoundException("Delivery Date is not null");
        
        var order = _mapper.Map<OrderCreateModel>(request);
        order.DealerId = dealerId;

        return await _orderRepository.CreateOrder(_mapper.Map<Order>(order));
    }

    public async Task<OrderDetailResponse> GetOrderDetailByOrderId(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByOrderId(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");
        
        var customer = await _customerRepository.GetCustomerById(order.CustomerId);
        if (customer == null)
            throw new KeyNotFoundException("Customer does not exist");


        var quote = await _quoteRepository.GetQuoteByContractId(order.ContractId);
        if (quote == null)
            throw new KeyNotFoundException("Quote not found");
        
        var vehicle = await _quoteRepository.GetVehicleByQuoteId(quote.QuoteId);
        if (vehicle == null)
            throw new KeyNotFoundException("Vehicle not found");

        var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quote.QuoteId);
        if (vehicleVersion == null)
            throw new KeyNotFoundException("Vehicle not found");

        return new OrderDetailResponse()
        {
            OrderId = order.OrderId,
            Name = customer.Name,
            Phone = customer.Phone ?? "Unknown",
            Email = customer.Email ?? "Unknown",
            Brand = vehicle.Brand,
            ModelName = vehicle.ModelName,
            Color = vehicleVersion.Color ?? "Unknown",
            DeliveryAddress = order.DeliveryAddress ?? "Unknown",
            DeliveryDate = order.DeliveryDate,
            Status = order.Status,
        };
    }

    public async Task<IEnumerable<OrderDetailResponse>> GetOrdersByDealerId(Guid dealerId)
    {
        if (dealerId == Guid.Empty)
            throw new KeyNotFoundException("Dealer does not exist");
        var dealer = await _dealerRepository.GetDealerByDealerId(dealerId);
        if (dealer == null)
            throw new KeyNotFoundException("Dealer not found");

        var orders = await _orderRepository.GetOrdersByDealerId(dealerId);
        
        return orders.Select(o => new OrderDetailResponse
        {
            OrderId = o.OrderId,
            Name = o.Contract?.Customer?.Name ?? "Unknown",
            Phone = o.Contract?.Customer?.Phone ?? "Unknown",
            Email = o.Contract?.Customer?.Email ?? "Unknown",
            Brand = o.Contract?.Quote?.VehicleVersion?.Vehicle?.Brand ?? "Unknown",
            ModelName = o.Contract?.Quote?.VehicleVersion?.Vehicle?.ModelName ?? "Unknown",
            Color = o.Contract?.Quote?.VehicleVersion?.Color ?? "Unknown",
            DeliveryAddress = o.DeliveryAddress ?? "Unknown",
            DeliveryDate = o.DeliveryDate,
            Status = o.Status
        });
    }

    public async Task<bool> UpdateOrderStatus(Guid orderId, OrderUpdateStatusRequest request)
    {
        if (orderId == Guid.Empty)
            throw new KeyNotFoundException("Order Id is not null");
        if(request.Status == null)
            throw new KeyNotFoundException("Status update is not null");

        var order = await _orderRepository.GetOrderByOrderId(orderId);
        if(order == null)
            throw new KeyNotFoundException("Order does not exist");
        order.Status = request.Status;

        return await _orderRepository.UpdateOrderStatus(order);
    }
}