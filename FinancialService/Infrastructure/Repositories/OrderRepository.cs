using FinancialService.Context;
using FinancialService.DTOs.Responses;
using FinancialService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            var baseQuery = _context.orders
                .AsNoTracking()
                .Where(o => o.delivery_date != null 
                            && o.delivery_date >= start 
                            && o.delivery_date < end);

            var completed = baseQuery.Where(o => CompletedStatuses.Contains(o.status));

            var revenue = await completed
                .Join(_context.contracts.AsNoTracking(), o => o.contract_id, c => c.contract_id, (o, c) => c.total_value)
                .SumAsync(cancellationToken);

            var carsSold = await completed.CountAsync(cancellationToken);

            // TODO: When PaidAmountVnd/TotalAmountVnd exist on orders, compute real debt.
            decimal customerDebt = 0m;

            return (revenue, carsSold, customerDebt);
        }

        // Monthly employee sales (sum of contract total_value) for completed/delivered orders
        public async Task<IEnumerable<EmployeeSalesDto>> GetEmployeeSalesAsync(int year, int month, CancellationToken ct)
        {
            var yearVal = year;
            var monthVal = month;

            var query = from o in _context.orders.AsNoTracking()
                        join c in _context.contracts.AsNoTracking() on o.contract_id equals c.contract_id
                        join q in _context.quotes.AsNoTracking() on c.quote_id equals q.quote_id into qleft
                        from q in qleft.DefaultIfEmpty()
                        join u in _context.users.AsNoTracking() on q.created_by_user equals u.user_id into uleft
                        from u in uleft.DefaultIfEmpty()
                        where o.delivery_date != null
                              && o.delivery_date.Value.Year == yearVal
                              && o.delivery_date.Value.Month == monthVal
                              && CompletedStatuses.Contains(o.status)
                        group new { c.total_value } by new { u.user_id, u.name } into g
                        orderby g.Sum(x => x.total_value) descending
                        select new EmployeeSalesDto(
                            g.Key.name,
                            g.Sum(x => x.total_value)
                        );

            return await query.ToListAsync(ct);
        }
    }
}
