using Application.ExceptionHandler;
using DealerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.DTOs.Requests.ContractDTOs;
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

    /// <summary>
    /// Lấy danh sách hợp đồng theo CustomerId.
    /// </summary>
    //[Authorize(Roles = "dealer_staff")]
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
    /// <summary>
    /// Tạo một contract mới
    /// </summary>
    [HttpPost]
    [Route("api/contracts")]
    public async Task<IActionResult> CreateContract(ContractCreateRequest request)
    {
        try
        {
            var isSuccess = await _contractService.CreateContract(request);
            return Ok(ApiResponse<bool>.Success(isSuccess));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFound(ex.Message));
        }
    }

    /// <summary>
    /// Lấy ra chi tiết của 1 contract
    /// </summary>
    [HttpGet]
    [Route("/contracts/{contractId}")]
    public async Task<IActionResult> GetContractByContractId(Guid contractId)
    {
        try
        {
            var contract = await _contractService.GetContractByContractId(contractId);
            return Ok(ApiResponse<ContractDetailResponse>.Success(contract)); 
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFound(ex.Message));
        }
    }
    
    /// <summary>
    /// Lấy ra các contract của dealer
    /// </summary>
    //[Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("/contracts/dealers/{dealerId}")]
    public async Task<IActionResult> GetContractByDealerId(Guid dealerId)
    {
        try
        {
            var contract = await _contractService.GetAllContractsByDealerId(dealerId);
            return Ok(ApiResponse<IEnumerable<ContractDealerResponse>>.Success(contract)); 
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFound(ex.Message));
        }
    }
}