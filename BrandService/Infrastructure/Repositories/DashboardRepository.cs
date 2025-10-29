using BrandService.Context;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class DashboardRepository
    {
        private readonly MyDbContext _context;

        public DashboardRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalVehicleVersionsAsync()
        {
            try
            {
                return await _context.VehicleVersions.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching total vehicle versions: {ex.Message}");
            }
        }

        public async Task<int> GetTotalStockQuantityAsync()
        {
            try
            {
                var brandTotal = await _context.BrandInventories
                    .SumAsync(b => (int?)b.StockQuantity ?? 0);

                var dealerTotal = await _context.Inventories
                    .SumAsync(i => (int?)i.StockQuantity ?? 0);

                return brandTotal + dealerTotal;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total stock: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalDealersAsync()
        {
            try
            {
                return await _context.Dealers.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving total dealers: {ex.Message}");
            }
        }

        public async Task<int> GetPendingTransferOrdersAsync()
        {
            try
            {
                return await _context.VehicleTransferOrders
                .CountAsync(t => t.Status == "processing" || t.Status == "pending");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving pending transfer orders: {ex.Message}");
            }
        }

        public async Task<int> GetPendingAllocationsAsync()
        {
            return await _context.VehicleAllocations
                .CountAsync(a => a.Status == "received");
        }
    }
}
