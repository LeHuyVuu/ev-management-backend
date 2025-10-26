using FinancialService.DTOs.Responses;
using FinancialService.Infrastructure.Repositories;

namespace FinancialService.Infrastructure.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        public OrderService(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<IEnumerable<SalesByModelResponse>> GetSalesByModelAsync()
            => _orderRepository.GetAllOrdersAsync();

        public Task<IEnumerable<EmployeeSalesDto>> GetEmployeeSalesAsync(int year, int month, CancellationToken ct)
            => _orderRepository.GetEmployeeSalesAsync(year, month, ct);

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? year, int? month, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var currStart = new DateOnly(y, m, 1);
            var nextMonth = currStart.AddMonths(1);
            var prevStart = currStart.AddMonths(-1);

            var (currRevenue, currCarsSold, currDebt) = await _orderRepository.GetMetricsAsync(currStart, nextMonth, ct);
            var (prevRevenue, prevCarsSold, prevDebt) = await _orderRepository.GetMetricsAsync(prevStart, currStart, ct);

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
                CustomerDebtVnd: currDebt,
                CustomerDebtMoMChangePct: CalcPctChange(prevDebt, currDebt)
            );
        }
    }
}
