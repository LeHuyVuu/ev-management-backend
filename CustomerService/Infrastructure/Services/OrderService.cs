using AutoMapper;
using CustomerService.DTOs.Responses.OrderDTOs;
using CustomerService.Infrastructure.Repositories;

namespace CustomerService.Infrastructure.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public OrderService(OrderRepository orderRepository, CustomerRepository customerRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
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
}