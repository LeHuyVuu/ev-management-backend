using FinancialService.Context;
using FinancialService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repositories
{
    public class InventoryReportRepository
    {
        private readonly MyDbContext _context;

        public InventoryReportRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryReportDto>> GetInventoryReportAsync(
            Guid? dealerId,
            string? sortBy,
            string? order)
        {
            var query = _context.inventories
                .Include(i => i.dealer)
                .Include(i => i.vehicle_version)
                .ThenInclude(vv => vv.vehicle)
                .AsQueryable();

            if (dealerId.HasValue)
                query = query.Where(i => i.dealer_id == dealerId.Value);

            var data = query.Select(i => new InventoryReportDto
            {
                DealerId = i.dealer_id,
                DealerName = i.dealer.name,
                Brand = i.vehicle_version.vehicle.brand,
                ModelName = i.vehicle_version.vehicle.model_name,
                VersionName = i.vehicle_version.version_name,
                StockQuantity = i.stock_quantity,
                LastUpdated = i.last_updated
            });

            bool isDesc = order?.ToLower() == "desc";

            data = sortBy?.ToLower() switch
            {
                "dealername" => isDesc ? data.OrderByDescending(d => d.DealerName)
                                       : data.OrderBy(d => d.DealerName),

                "brand" => isDesc ? data.OrderByDescending(d => d.Brand)
                                  : data.OrderBy(d => d.Brand),

                "modelname" => isDesc ? data.OrderByDescending(d => d.ModelName)
                                      : data.OrderBy(d => d.ModelName),

                "versionname" => isDesc ? data.OrderByDescending(d => d.VersionName)
                                        : data.OrderBy(d => d.VersionName),

                "stockquantity" or "stock" => isDesc ? data.OrderByDescending(d => d.StockQuantity)
                                                     : data.OrderBy(d => d.StockQuantity),

                "lastupdated" or "date" => isDesc ? data.OrderByDescending(d => d.LastUpdated)
                                                  : data.OrderBy(d => d.LastUpdated),

                _ => data.OrderByDescending(d => d.StockQuantity) // default sort
            };

            return await data.ToListAsync();
        }
    }
}
