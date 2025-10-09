using FinancialService.Model;
using FinancialService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly InventoryReportService _inventoryReportService;

        public ReportsController(InventoryReportService inventoryReportService)
        {
            _inventoryReportService = inventoryReportService;
        }

        /// <summary>
        /// Báo cáo tồn kho (có lọc đại lý + sort linh hoạt)
        /// </summary>
        /// <param name="dealerId">ID đại lý (tùy chọn)</param>
        /// <param name="sortBy">dealerName | brand | modelName | versionName | stockQuantity | lastUpdated</param>
        /// <param name="order">asc | desc</param>
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport(
            [FromQuery] Guid? dealerId,
            [FromQuery] string? sortBy = "stockQuantity",
            [FromQuery] string? order = "desc")
        {
            var response = await _inventoryReportService.GetInventoryReportAsync(dealerId, sortBy, order);
            return StatusCode(response.Status, response);
        }
    }
}