using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Entities;
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
    
    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomers();
        if (customers != null)
        {
            return Ok(customers);
        }
        return NotFound();
    }

    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/customers/{customerId}")]
    public async Task<IActionResult> GetCustomerDetail(Guid customerId)
    {
        var customer = await _customerService.GetCustomerDetail(customerId);
        if (customer != null)
        {
            return Ok(customer);
        }
        return NotFound();
    }

    // [Authorize(Roles = "dealer_staff")]
    [HttpPost]
    [Route("api/customers")]
    public async Task<IActionResult> CreateCustomer(CustomerCreateRequest request)
    {
        bool isSuccess = await _customerService.CreateCustomer(request);
        if (isSuccess)
        {
            return Ok();
        }
        return BadRequest();
    }

    // [Authorize(Roles = "dealer_staff")]
    [HttpPut]
    [Route("api/customers")]
    public async Task<IActionResult> UpdateCustomer(CustomerUpdateRequest request)
    {
        bool isSuccess = await _customerService.UpdateCustomer(request);
        if (isSuccess)
        {
            return Ok();
        }
        return BadRequest();
    }
}