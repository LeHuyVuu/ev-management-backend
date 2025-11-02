using FinancialService.DTOs.Responses;
using FinancialService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken cancellationToken)
        {
            Guid? dealerId = null;
            var dealerIdClaim = User.FindFirstValue("DealerId");
            if (!string.IsNullOrEmpty(dealerIdClaim) && Guid.TryParse(dealerIdClaim, out var parsed))
            {
                dealerId = parsed;
            }

            try
            {
                var dto = await _orderService.GetDashboardSummaryAsync(cancellationToken, dealerId);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Doanh số theo nhân viên trong phần tổng quan & doanh thu
        /// </summary>
        [HttpGet("employee-sales")]
        public async Task<ActionResult<IEnumerable<EmployeeSalesDto>>> GetEmployeeSales(CancellationToken ct)
        {
            Guid? dealerId = null;
            var dealerIdClaim = User.FindFirstValue("DealerId");
            if (!string.IsNullOrEmpty(dealerIdClaim) && Guid.TryParse(dealerIdClaim, out var parsed))
            {
                dealerId = parsed;
            }

            // Conservative detection for dealer-staff role: require explicit token
            Guid? currentUserId = null;
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? string.Empty;

            bool isDealerStaff = false;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                // split common separators and check tokens exactly to avoid accidental matches
                var tokens = roleClaim.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim().ToLowerInvariant());
                isDealerStaff = tokens.Any(t => t == "dealer-staff" || t == "dealer_staff" || t == "dealerstaff");
            }

            // Also support explicit claim name IsDealerStaff=true
            var isDealerStaffFlag = User.FindFirstValue("IsDealerStaff");
            if (!isDealerStaff && !string.IsNullOrEmpty(isDealerStaffFlag) && bool.TryParse(isDealerStaffFlag, out var flagVal) && flagVal)
            {
                isDealerStaff = true;
            }

            if (isDealerStaff)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUser))
                {
                    currentUserId = parsedUser;
                }
            }

            try
            {
                var result = await _orderService.GetEmployeeSalesAsync(dealerId, currentUserId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Doanh thu theo tháng trong năm dựa trên các order đã hoàn tất của dealer hiện tại
        /// </summary>
        /// <param name="year">Năm cần báo cáo</param>
        [HttpGet("monthly-revenue")]
        public async Task<ActionResult<IEnumerable<MonthlyRevenueDto>>> GetMonthlyRevenue([FromQuery] int year, CancellationToken ct)
        {
            Guid? dealerId = null;
            var dealerIdClaim = User.FindFirstValue("DealerId");
            if (!string.IsNullOrEmpty(dealerIdClaim) && Guid.TryParse(dealerIdClaim, out var parsed))
            {
                dealerId = parsed;
            }

            if (year <= 0)
            {
                year = DateTime.Now.Year;
            }

            try
            {
                var result = await _orderService.GetMonthlyRevenueAsync(year, dealerId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
