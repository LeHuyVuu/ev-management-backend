using BrandService.DTOs.Responses.ReportDTOs;
using BrandService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly SalesReportService _salesService;

        public ReportsController(SalesReportService salesService)
        {
            _salesService = salesService;
        }

        /// <summary>
        /// Get sales report by filters (month, quarter, year, region, dealer)
        /// </summary>
        [HttpGet("sales")]
        [ProducesResponseType(typeof(SalesReportResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<SalesReportResponse>> GetSalesReport(
            [FromQuery] int? month,
            [FromQuery] int? quarter,
            [FromQuery] int? year,
            [FromQuery] string? region,
            [FromQuery] Guid? dealerId)
        {
            var result = await _salesService.GetSalesReportAsync(month, quarter, year, region, dealerId);
            return Ok(result);
        }
    }
}
