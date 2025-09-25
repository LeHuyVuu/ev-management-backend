using DealerService.Models;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure.Controller;

[ApiController]
public class ContractController : ControllerBase
{
    private readonly ContractService _contractService;

    public ContractController(ContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    [Route("/customers/{customerId}/contracts")]
    public async Task<IActionResult> GetAllContractsByCustomerId(Guid customerId)
    {
        try
        {
            var contracts = await _contractService.GetAllContractsByCustomerId(customerId);
            return Ok(ApiResponse<IEnumerable<ContractCustomerResponse>>.Success(contracts));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
    }
}