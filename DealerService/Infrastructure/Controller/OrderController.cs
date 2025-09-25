using DealerService.Models;
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
    
    /// <summary>
    /// Lấy danh sách Order theo CustomerId
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("customers/{customerId}/orders")]
    public async Task<IActionResult> GetAllOrdersByCustomerId(Guid customerId)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersByCustomerId(customerId);
            return Ok(ApiResponse<IEnumerable<OrderCustomerResponse>>.Success(orders));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
    }
}