using FinancialService.DTOs.Responses;
using FinancialService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace FinancialService.Infrastructure.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;

        public OrderService(OrderRepository orderRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
        }

        public Task<IEnumerable<SalesByModelResponse>> GetSalesByModelAsync()
            => _orderRepository.GetAllOrdersAsync();

        // Always use current month for employee sales
        public async Task<IEnumerable<EmployeeSalesDto>> GetEmployeeSalesAsync(Guid? dealerId, CancellationToken ct)
        {
            if (!dealerId.HasValue || dealerId.Value == Guid.Empty)
                throw new InvalidOperationException("DealerId is required in the token.");

            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            // Pull raw sales per-employee for dealer in current month
            var raw = await _orderRepository.GetEmployeeSalesAsync(year, month, dealerId.Value, ct);

            // Get dealer monthly target; fallback to config if not present
            var monthStart = new DateOnly(year, month, 1);
            var dealerTarget = await _orderRepository.GetMonthlyTargetAsync(monthStart, dealerId.Value, ct);
            if (dealerTarget <= 0)
            {
                dealerTarget = _configuration.GetValue<decimal?>("Sales:MonthlyTargetVnd") ?? 1_200_000_000m;
            }

            static string PerfStatus(decimal pct)
                => pct >= 120m ? "Xuất sắc"
                 : pct >= 100m ? "Tốt"
                 : pct >= 90m  ? "Đang tiến triển"
                 : "Cần cải thiện";

            // TargetAchievedPct: return raw monthly target (no calculation). Status still based on ratio for context.
            return raw.Select(e =>
            {
                var ratioPct = dealerTarget <= 0 ? 0 : (e.TotalSalesVnd / dealerTarget) * 100m;
                return new EmployeeSalesDto(e.EmployeeName, e.TotalSalesVnd, dealerTarget, PerfStatus(ratioPct));
            }).ToList();
        }

        // Always use current month for dashboard summary
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct, Guid? dealerId = null)
        {
            if (!dealerId.HasValue || dealerId.Value == Guid.Empty)
                throw new InvalidOperationException("DealerId is required in the token.");

            var now = DateTime.Now;
            var y = now.Year;
            var m = now.Month;

            var currStart = new DateOnly(y, m, 1);
            var nextMonth = currStart.AddMonths(1);
            var prevStart = currStart.AddMonths(-1);

            var (currRevenue, currCarsSold, currDebt) = await _orderRepository.GetMetricsAsync(currStart, nextMonth, ct);
            var (prevRevenue, prevCarsSold, prevDebt) = await _orderRepository.GetMetricsAsync(prevStart, currStart, ct);

            var monthlyTarget = await _orderRepository.GetMonthlyTargetAsync(currStart, dealerId.Value, ct);
            if (monthlyTarget <= 0)
            {
                monthlyTarget = _configuration.GetValue<decimal?>("Sales:MonthlyTargetVnd") ?? 1_200_000_000m;
            }

            // Keep badge based on achievement ratio for additional context
            var ratioPct = monthlyTarget <= 0 ? 0 : (currRevenue / monthlyTarget) * 100m;
            var targetBadge = ratioPct >= 100m ? "Đạt"
                                : ratioPct >= 80m ? "Gần đạt"
                                : "Chưa đạt";

            static decimal CalcPctChange(decimal prev, decimal curr)
            {
                if (prev == 0 && curr > 0) return 100m;
                if (prev == 0 && curr == 0) return 0m;
                return ((curr - prev) / prev) * 100m;
            }

            return new DashboardSummaryDto(
                TotalMonthlyRevenueVnd: currRevenue,
                RevenueMoMChangePct: CalcPctChange(prevRevenue, currRevenue),
                CarsSoldThisMonth: currCarsSold,
                CarsSoldMoMChangePct: CalcPctChange(prevCarsSold, currCarsSold),
                TargetAchievedPct: monthlyTarget,
                TargetBadge: targetBadge,
                CustomerDebtVnd: currDebt,
                CustomerDebtMoMChangePct: CalcPctChange(prevDebt, currDebt)
            );
        }

        // Monthly revenue by month for a dealer in a year
        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year, Guid? dealerId, CancellationToken ct)
        {
            if (!dealerId.HasValue || dealerId.Value == Guid.Empty)
                throw new InvalidOperationException("DealerId is required in the token.");

            return await _orderRepository.GetMonthlyRevenueByYearAsync(year, dealerId.Value, ct);
        }
    }
}
