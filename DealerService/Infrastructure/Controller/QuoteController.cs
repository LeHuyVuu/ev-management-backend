using Application.ExceptionHandler;
using DealerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs.Responses.QuoteDTOs;
using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure.Controller;

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
    }
}