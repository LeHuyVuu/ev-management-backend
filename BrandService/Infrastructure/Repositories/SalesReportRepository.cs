using BrandService.Context;
using BrandService.DTOs.Responses.ReportDTOs;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class SalesReportRepository
    {
        private readonly MyDbContext _context;

        public SalesReportRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalesByModelRegion>> GetSalesByModelAndRegionAsync(
            int? month, int? quarter, int? year, string? region, Guid? dealerId)
        {
            try
            {
                var query = from o in _context.Orders
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
                                ModelName = v.ModelName,
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
    }
}
