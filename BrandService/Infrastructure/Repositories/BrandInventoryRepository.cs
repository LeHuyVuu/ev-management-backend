using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class BrandInventoryRepository
    {
        private readonly MyDbContext _context;
        public BrandInventoryRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddInventoryAsync(BrandInventory inventory)
        {
            try
            {
                _context.BrandInventories.Add(inventory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateStockAsync(Guid vehicleVersionId, int deltaQuantity)
        {
            var existing = await _context.BrandInventories
                .FirstOrDefaultAsync(bi => bi.VehicleVersionId == vehicleVersionId);

            // Không có record nào cho vehicleVersionId này
            if (existing == null)
                return false;

            var currentStock = existing.StockQuantity ?? 0;
            var newStock = currentStock + deltaQuantity;

            // Không thể trừ nhiều hơn số lượng hiện có
            if (newStock < 0)
                return false;

            existing.StockQuantity = newStock;

            _context.BrandInventories.Update(existing);
            await _context.SaveChangesAsync();

            return true;
        }

        
    }
}
