using System.Data;
using System.Security.Claims;
using CustomerService.DTOs.Requests.CustomerDTOs;
using CustomerService.DTOs.Responses.CustomerDTOs;
using CustomerService.Infrastructure.Services;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerServiceClass = CustomerService.Infrastructure.Services.CustomerService;

namespace CustomerService.Infrastructure.Controller;

[ApiController]
public class CustomerController : ControllerBase
{
    private readonly CustomerServiceClass _customerService;

    public CustomerController(CustomerServiceClass customerService)
    {
        _customerService = customerService;
    }
    
    /// <summary>
    /// Lấy danh sách khách hàng dựa theo dealer đang đăng nhập
    /// </summary>
    //[Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/customers")]
    public async Task<IActionResult> GetCustomersByDealerId()
    {
        try
        {
            Guid dealerId = Guid.Parse(User.FindFirstValue("DealerId"));
            var customers = await _customerService.GetCustomersByDealerId(dealerId);
            return Ok(ApiResponse<IEnumerable<CustomerBasicResponse>>.Success(customers));
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
    /// Lấy chi tiết khách hàng (profile) theo CustomerId.
    /// </summary>
    //[Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/customers/{customerId}")]
    public async Task<IActionResult> GetCustomerDetail(Guid customerId)
    {
        try
        {
            var customer = await _customerService.GetCustomerDetail(customerId);
            return Ok(ApiResponse<CustomerDetailResponse>.Success(customer));
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
    /// Tạo mới khách hàng.
    /// </summary>
    [Authorize]
    [HttpPost]
    [Route("api/customers")]
    public async Task<IActionResult> CreateCustomer(CustomerCreateRequest request)
    {
        try
        {
            Guid dealerId = Guid.Parse(User.FindFirstValue("DealerId"));
            if (dealerId == Guid.Empty)
                return NotFound(ApiResponse<string>.NotFound("You don't have a dealerId to perform this action"));

            bool isSuccess = await _customerService.CreateCustomer(dealerId, request);
            return Ok(ApiResponse<bool>.Success(isSuccess, "Customer was created successfully"));
        }
        catch (DuplicateNameException ex)
        {
            return BadRequest(ApiResponse<string>.Duplicate(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật profile của khách hàng.
    /// </summary>
    //[Authorize(Roles = "dealer_staff")]
    [HttpPut]
    [Route("api/customers")]
    public async Task<IActionResult> UpdateCustomer(CustomerUpdateRequest request)
    {
        try
        {
            bool isSuccess = await _customerService.UpdateCustomer(request);
            return Ok(ApiResponse<bool>.Success(isSuccess, "Customer was updated successfully"));
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