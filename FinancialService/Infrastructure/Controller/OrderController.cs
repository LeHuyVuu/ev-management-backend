using FinancialService.DTOs.Responses;
using FinancialService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Bảng doanh số theo mẫu xe
        /// </summary>
        [HttpGet("sales-by-model")]
        public async Task<IActionResult> GetSalesByModelAsync()
        {
            var result = await _orderService.GetSalesByModelAsync();
            return Ok(result);
        }

        /// <summary>
        /// Dashboard sumary (các card trong phần tổng quan & doanh thu)
        /// </summary>
        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummaryAsync([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
        {
            var dto = await _orderService.GetDashboardSummaryAsync(year, month, cancellationToken);
            return Ok(dto);
        }

        /// <summary>
        /// Doanh số theo nhân viên trong phần tổng quan & doanh thu
        /// </summary>
        [HttpGet("employee-sales")]
        public async Task<ActionResult<IEnumerable<EmployeeSalesDto>>> GetEmployeeSales([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;
            var result = await _orderService.GetEmployeeSalesAsync(y, m, ct);
            return Ok(result);
        }
    }
}
