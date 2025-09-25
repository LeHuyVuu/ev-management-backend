using AutoMapper;
using ProductService.Context;
using ProductService.DTOs;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public OrderService(OrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderCustomerResponse>> GetAllOrdersByCustomerId(Guid customerId)
    {
        bool isExist = await _customerRepository.CustomerExists(customerId);
        if (!isExist)
            throw new KeyNotFoundException("Customer does not exist!");
        var orders = await _orderRepository.GetAllOrdersByCustomerId(customerId);
        return _mapper.Map<IEnumerable<OrderCustomerResponse>>(orders);
    }
}