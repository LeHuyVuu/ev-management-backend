using System.Data;
using DealerService.Models;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure.Controller;

[ApiController]
public class CustomerController : ControllerBase
{
    private readonly CustomerService _customerService;

    public CustomerController(CustomerService customerService)
    {
        _customerService = customerService;
    }
    
    /// <summary>
    /// Lấy danh sách khách hàng.
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomers();
            return Ok(ApiResponse<IEnumerable<CustomerBasicResponse>>.Success(customers));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
    }

    /// <summary>
    /// Lấy chi tiết khách hàng (profile) theo CustomerId.
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
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
    }

    /// <summary>
    /// Tạo mới khách hàng.
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpPost]
    [Route("api/customers")]
    public async Task<IActionResult> CreateCustomer(CustomerCreateRequest request)
    {
        try
        {
            bool isSuccess = await _customerService.CreateCustomer(request);
            return Ok(ApiResponse<bool>.Success(isSuccess, "Customer was created successfully"));
        }
        catch (DuplicateNameException ex)
        {
            return BadRequest(ApiResponse<string>.Duplicate(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật profile của khách hàng.
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpPut]
    [Route("api/customers")]
    public async Task<IActionResult> UpdateCustomer(CustomerUpdateRequest request)
    {
        try
        {
            bool isSuccess = await _customerService.UpdateCustomer(request);;
            return Ok(ApiResponse<bool>.Success(isSuccess, "Customer was created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ApiResponse<string>.Duplicate(ex.Message));
        }
    }
}