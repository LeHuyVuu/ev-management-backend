using FinancialService.Context;
using FinancialService.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Infrastructure.Repositories
{
    public class OrderRepository
    {
        private readonly MyDbContext _context;
        private static readonly HashSet<string> CompletedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "delivered", "completed"
        };

        public OrderRepository(MyDbContext context)
        {
            _context = context;
        }

        // Return sales count grouped by car model (Brand + ModelName) for successful orders
        public async Task<IEnumerable<SalesByModelResponse>> GetAllOrdersAsync()
        {
            var successStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "delivered", "completed"
            };

            var result = await _context.orders
                .AsNoTracking()
                .Where(o => successStatuses.Contains(o.status)
                            && o.contract != null
                            && o.contract.quote != null)
                .GroupBy(o => o.contract.quote.vehicle_version.vehicle.brand + " " +
                               o.contract.quote.vehicle_version.vehicle.model_name)
                .Select(g => new SalesByModelResponse
                {
                    ModelCar = g.Key,
                    UnitsSold = g.Count()
                })
                .ToListAsync();

            return result;
        }

        // Metrics for a given date range [start, end)
        public async Task<(decimal revenue, int carsSold, decimal customerDebt)> GetMetricsAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken)
        {
            var completed = _context.orders
                .AsNoTracking()
                .Where(o => o.delivery_date != null && o.delivery_date >= start && o.delivery_date < end)
                .Where(o => CompletedStatuses.Contains(o.status));

            var revenue = await completed
                .Join(_context.contracts.AsNoTracking(), o => o.contract_id, c => c.contract_id, (o, c) => c.total_value)
                .SumAsync(cancellationToken);

            var carsSold = await completed.CountAsync(cancellationToken);

            // TODO: When PaidAmountVnd/TotalAmountVnd exist on orders, compute real debt.
            decimal customerDebt = 0m;

            return (revenue, carsSold, customerDebt);
        }

        // Monthly employee sales for a dealer using exact Year/Month filter and simple LINQ
        public async Task<IEnumerable<EmployeeSalesDto>> GetEmployeeSalesAsync(int year, int month, Guid dealerId, CancellationToken ct)
        {
            // Employees (users) belonging to the dealer
            var employees = _context.users
                .AsNoTracking()
                .Where(u => u.dealer_id == dealerId)
                .Select(u => new { u.user_id, u.name });

            // Aggregate monthly sales per employee (by quote.created_by_user)
            var monthlySales =
                from o in _context.orders.AsNoTracking()
                join c in _context.contracts.AsNoTracking() on o.contract_id equals c.contract_id
                join q in _context.quotes.AsNoTracking() on c.quote_id equals q.quote_id
                where o.dealer_id == dealerId
                      && o.delivery_date != null
                      && o.delivery_date.Value.Year == year
                      && o.delivery_date.Value.Month == month
                      && CompletedStatuses.Contains(o.status)
                group c by q.created_by_user into g
                select new { UserId = g.Key, Total = g.Sum(x => x.total_value) };

            var projected =
                from u in employees
                join s in monthlySales on u.user_id equals s.UserId into gj
                from s in gj.DefaultIfEmpty()
                select new
                {
                    u.name,
                    Total = (decimal?)(s != null ? s.Total : null) ?? 0m
                };

            var ordered = projected
                .OrderByDescending(x => x.Total)
                .Select(x => new EmployeeSalesDto(
                    x.name,
                    x.Total,
                    0m,
                    string.Empty
                ));

            return await ordered.ToListAsync(ct);
        }

        // Latest monthly dealer target (start_date <= currStart) for a specific dealer
        public async Task<decimal> GetMonthlyTargetAsync(DateOnly currStart, Guid dealerId, CancellationToken ct)
        {
            var monthly = "monthly";

            var target = await _context.dealer_targets
                .AsNoTracking()
                .Where(t => t.dealer_id == dealerId && t.period.ToLower() == monthly && t.start_date <= currStart)
                .OrderByDescending(t => t.start_date)
                .Select(t => (decimal?)t.target_amount)
                .FirstOrDefaultAsync(ct) ?? 0m;

            return target;
        }

        // Monthly revenue for a specific dealer in a given year
        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueByYearAsync(int year, Guid dealerId, CancellationToken ct)
        {
            var query =
                from o in _context.orders.AsNoTracking()
                join c in _context.contracts.AsNoTracking() on o.contract_id equals c.contract_id
                where o.dealer_id == dealerId
                      && o.delivery_date != null
                      && o.delivery_date.Value.Year == year
                      && CompletedStatuses.Contains(o.status)
                group c by o.delivery_date!.Value.Month into g
                select new { Month = g.Key, Revenue = g.Sum(x => x.total_value), Units = g.Count() };

            var rows = await query.ToListAsync(ct);
            var map = rows.ToDictionary(x => x.Month, x => new MonthlyRevenueDto(x.Month, x.Revenue, x.Units));

            var result = new List<MonthlyRevenueDto>(12);
            for (int m = 1; m <= 12; m++)
            {
                if (map.TryGetValue(m, out var val))
                {
                    result.Add(val);
                }
                else
                {
                    result.Add(new MonthlyRevenueDto(m, 0m, 0));
                }
            }

            return result;
        }
    }
}
