using BrandService.DTOs.Responses.ReportDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _salesService;

        public ReportsController(ReportService salesService)
        {
            _salesService = salesService;
        }

        /// <summary>
        /// Get sales report by filters (month, quarter, year, region, dealer)
        /// </summary>
        [HttpGet("sales")]
        [ProducesResponseType(typeof(ApiResponse<SalesReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesReportResponse>> GetSalesReport(
            [FromQuery] int? month,
            [FromQuery] int? quarter,
            [FromQuery] int? year,
            [FromQuery] string? region,
            [FromQuery] Guid? dealerId)
        {
            var result = await _salesService.GetSalesReportAsync(month, quarter, year, region, dealerId);
            return Ok(ApiResponse<SalesReportResponse>.Success(result));
        }

        /// <summary>
        /// Get top dealers by sales performance.
        /// </summary>
        [HttpGet("top-dealers")]
        [ProducesResponseType(typeof(ApiResponse<TopDealersReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TopDealersReportResponse>> GetTopDealers()
        {
            var result = await _salesService.GetTopDealersAsync();
            return Ok(ApiResponse<TopDealersReportResponse>.Success(result));
        }

        /// <summary>
        /// Get current inventory across all dealers.
        /// </summary>
        /// <returns></returns>
        [HttpGet("current-inventory")]
        [ProducesResponseType(typeof(ApiResponse<List<CurrentInventoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CurrentInventoryResponse>>> GetCurrentDealersInventory()
        {
            var result = await _salesService.GetCurrentDealersInventoryAsync();
            return Ok(ApiResponse<List<CurrentInventoryResponse>>.Success(result));
        }
    }
}
