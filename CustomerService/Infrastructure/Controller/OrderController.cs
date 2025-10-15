using System.Security.Claims;
using CustomerService.DTOs.Requests.OrderDTOs;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerService.DTOs.Responses.OrderDTOs;
using CustomerService.Infrastructure.Services;

namespace CustomerService.Infrastructure.Controller;

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
    //[Authorize(Roles = "dealer_staff")]
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
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }
    
    /// <summary>
    /// Tạo đơn hàng cho khác hàng
    /// </summary>
    [HttpPost]
    [Route("api/customers/orders")]
    public async Task<IActionResult> CreateOrder([FromBody]OrderCreateRequest request)
    {
        try
        {
            Guid dealerId = Guid.Parse(User.FindFirstValue("DealerId"));
            var order = await _orderService.CreateOrder(dealerId, request);
            return Ok(ApiResponse<bool>.Success(true));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }

    /// <summary>
    /// Lấy OrderDetail bằng OrderId
    /// </summary>
    [HttpGet]
    [Route("api/orders/{orderId}")]
    public async Task<IActionResult> GetOrderDetailByOrderId(Guid orderId)
    {
        try
        {
            var orderDetail = await _orderService.GetOrderDetailByOrderId(orderId);
            return Ok(ApiResponse<OrderDetailResponse>.Success(orderDetail));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }
    
    /// <summary>
    /// Lấy đơn hàng mà dealer hiện tại đăng nhập đang quản lý
    /// </summary>
    [HttpGet]
    [Route("api/orders")]
    public async Task<IActionResult> GetOrdersByDealerId()
    {
        try
        {
            Guid dealerId = Guid.Parse(User.FindFirstValue("DealerId"));
            var orders = await _orderService.GetOrdersByDealerId(dealerId);
            return Ok(ApiResponse<IEnumerable<OrderDetailResponse>>.Success(orders));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }
    
    /// <summary>
    /// Cập nhật trạng thái của đơn hàng
    /// </summary>
    [HttpPatch]
    [Route("api/orders/{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody]OrderUpdateStatusRequest request)
    {
        try
        {
            var orders = await _orderService.UpdateOrderStatus(orderId, request);
            return Ok(ApiResponse<bool>.Success(orders));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }
}