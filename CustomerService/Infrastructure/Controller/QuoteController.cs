using CustomerService.ExceptionHandler;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using CustomerService.DTOs.Requests.QuoteDTOs;
using CustomerService.DTOs.Responses.QuoteDTOs;
using CustomerService.Infrastructure.Services;

namespace CustomerService.Infrastructure.Controller;

public class QuoteController : ControllerBase
{
    private readonly QuoteService _quoteService;

    public QuoteController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }
    /// <summary>
    /// Lấy ra thông tin chi tiết của 1 Quote theo quoteId
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpGet]
    [Route("api/quotes/{id}")]
    public async Task<IActionResult> GetQuoteDetailByQuoteId(Guid id)
    {
        try
        {
            var quote = await _quoteService.GetQuoteDetailByQuoteId(id);
            return Ok(ApiResponse<QuoteDetailResponse>.Success(quote));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<string>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }

    /// <summary>
    /// Lấy ra các Quotes mà Dealer đảm nhận, theo dealerId
    /// </summary>
    // [Authorize(Roles = "evm_staff")]
    [HttpGet]
    [Route("api/quotes/dealers/{dealerId}")]
    public async Task<IActionResult> GetQuotesByDealerId(Guid dealerId)
    {
        try
        {
            var quotes = await _quoteService.GetQuotesByDealerId(dealerId);

            if (!quotes.Any())
                return NotFound(ApiResponse<IEnumerable<QuoteBasicResponse>>.NotFound("No quotes found"));

            return Ok(ApiResponse<IEnumerable<QuoteBasicResponse>>.Success(quotes));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }

    /// <summary>
    /// Cập nhật lại thông tin của 1 quote thông qua quoteId
    /// </summary>
    // [Authorize(Roles = "dealer_staff")]
    [HttpPut]
    [Route("api/quotes/{quoteId}")]
    public async Task<IActionResult> UpdateQuoteByQuoteId(Guid quoteId, [FromBody]QuoteUpdateRequest request)
    {
        try
        {
            bool isSuccess = await _quoteService.UpdateQuoteByQuoteId(quoteId, request);
            return Ok(ApiResponse<bool>.Success(isSuccess));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.InternalError(ex.Message));
        }
    }
}