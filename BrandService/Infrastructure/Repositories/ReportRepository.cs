using BrandService.Context;
using BrandService.DTOs.Responses.ReportDTOs;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class ReportRepository
    {
        private readonly MyDbContext _context;

        public ReportRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalesByModelRegion>> GetSalesByModelAndRegionAsync(
            int? month, int? quarter, int? year, string? region, Guid? dealerId)
        {
            try
            {
                var query = from o in _context.Orders
                            where o.Status == "completed"
                            join c in _context.Contracts on o.ContractId equals c.ContractId
                            join d in _context.Dealers on o.DealerId equals d.DealerId
                            join q in _context.Quotes on c.QuoteId equals q.QuoteId
                            join vv in _context.VehicleVersions on q.VehicleVersionId equals vv.VehicleVersionId
                            join v in _context.Vehicles on vv.VehicleId equals v.VehicleId
                            select new
                            {
                                o.DeliveryDate,
                                d.Region,
                                DealerId = d.DealerId,
                                ModelName = v.Brand + " " + v.ModelName + " " + vv.VersionName + " " + vv.Color,
                                Revenue = c.TotalValue
                            };

                if (year.HasValue)
                    query = query.Where(x => x.DeliveryDate.HasValue && x.DeliveryDate.Value.Year == year.Value);

                if (month.HasValue)
                    query = query.Where(x => x.DeliveryDate.HasValue && x.DeliveryDate.Value.Month == month.Value);

                if (quarter.HasValue)
                {
                    int startMonth = (quarter.Value - 1) * 3 + 1;
                    int endMonth = startMonth + 2;
                    query = query.Where(x =>
                        x.DeliveryDate.HasValue &&
                        x.DeliveryDate.Value.Month >= startMonth &&
                        x.DeliveryDate.Value.Month <= endMonth);
                }

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(x => x.Region == region);

                if (dealerId.HasValue)
                    query = query.Where(x => x.DealerId == dealerId.Value);

                var result = await query
                    .GroupBy(x => new { x.ModelName, x.Region })
                    .Select(g => new SalesByModelRegion
                    {
                        Model = g.Key.ModelName,
                        Region = g.Key.Region,
                        TotalSales = g.Count(),
                        TotalRevenue = g.Sum(x => x.Revenue)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error when get sales reports.");
            }
        }

        public async Task<List<TopDealerResponse>> GetTopDealersAsync()
        {
            try
            {
                // Giai đoạn hiện tại
                var currentQuery = from o in _context.Orders
                                   where o.Status == "completed"
                                   join c in _context.Contracts on o.ContractId equals c.ContractId
                                   join d in _context.Dealers on o.DealerId equals d.DealerId
                                   select new
                                   {
                                       d.DealerId,
                                       d.Name,
                                       d.Region,
                                       c.TotalValue,
                                       o.DeliveryDate
                                   };

                var currentSales = await currentQuery
                    .GroupBy(x => new { x.DealerId, x.Name, x.Region })
                    .Select(g => new
                    {
                        g.Key.DealerId,
                        DealerName = g.Key.Name,
                        g.Key.Region,
                        TotalSales = g.Sum(x => x.TotalValue)
                    })
                    .ToListAsync();

                // Giai đoạn trước (để tính growth)
                DateTime now = DateTime.UtcNow;
                int prevMonth = now.Month - 1;
                int prevYear = now.Year;

                if (prevMonth <= 0)
                {
                    prevMonth = 12;
                    prevYear--;
                }

                var previousQuery = from o in _context.Orders
                                    where o.Status == "completed"
                                    join c in _context.Contracts on o.ContractId equals c.ContractId
                                    join d in _context.Dealers on o.DealerId equals d.DealerId
                                    select new
                                    {
                                        d.DealerId,
                                        d.Name,
                                        d.Region,
                                        c.TotalValue,
                                        o.DeliveryDate
                                    };

                previousQuery = previousQuery.Where(x =>
                    x.DeliveryDate.HasValue &&
                    x.DeliveryDate.Value.Month == prevMonth &&
                    x.DeliveryDate.Value.Year == prevYear);

                var previousSales = await previousQuery
                    .GroupBy(x => new { x.DealerId })
                    .Select(g => new
                    {
                        g.Key.DealerId,
                        TotalSales = g.Sum(x => x.TotalValue)
                    })
                    .ToListAsync();

                // tính tăng trưởng (%)
                var result = currentSales
                    .Select(cs =>
                    {
                        var prev = previousSales.FirstOrDefault(p => p.DealerId == cs.DealerId);
                        decimal prevValue = prev?.TotalSales ?? 0;
                        decimal growthPercent = prevValue == 0 ? 100 : ((cs.TotalSales - prevValue) / prevValue) * 100;

                        return new TopDealerResponse
                        {
                            DealerName = cs.DealerName,
                            Region = cs.Region,
                            TotalSales = cs.TotalSales,
                            GrowthPercent = $"{(growthPercent >= 0 ? "+" : "")}{growthPercent:F0}%"
                        };
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(10)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error when get top dealers.");
            }
        }

        public async Task<List<CurrentInventoryResponse>> GetCurrentDealersInventoryAsync()
        {
            try
            {
                return await _context.Inventories
                .Include(i => i.VehicleVersion)
                    .ThenInclude(vv => vv.Vehicle)
                .GroupBy(i => new
                {
                    i.VehicleVersionId,
                    Brand = i.VehicleVersion!.Vehicle!.Brand,
                    ModelName = i.VehicleVersion!.Vehicle!.ModelName,
                    VersionName = i.VehicleVersion!.VersionName
                })
                .Select(g => new CurrentInventoryResponse
                {
                    Model = g.Key.Brand + " " + g.Key.ModelName,
                    VersionName = g.Key.VersionName,
                    DealersStock = g.Sum(x => x.StockQuantity),
                    LastUpdated = g.Max(x => x.LastUpdated)
                })
                .OrderByDescending(x => x.LastUpdated)
                .ThenByDescending(x => x.DealersStock)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error when get current dealers inventory.");
            }
        }
    }
}
