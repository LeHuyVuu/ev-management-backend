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
    public async Task<IEnumerable<ContractCustomerResponse>> GetAllContractsByCustomerId(Guid customerId)
    {
        return await _contractService.GetAllContractsByCustomerId(customerId);
    }
}