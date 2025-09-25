using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure.Controller;

public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }
    
    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("customers/{customerId}/orders")]
    public async Task<IEnumerable<OrderCustomerResponse>> GetAllOrdersByCustomerId(Guid customerId)
    {
        return await _orderService.GetAllOrdersByCustomerId(customerId);
    }
}