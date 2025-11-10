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

        public async Task<BrandInventory?> GetByVehicleVersionIdAsync(Guid vehicleVersionId)
        {
            try
            {
                return await _context.BrandInventories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(bi => bi.VehicleVersionId == vehicleVersionId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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


        public async Task UpdateInventoryAsync(BrandInventory inventory)
        {
            try
            {
                if (inventory.StockQuantity < 0)
                {
                    throw new BadRequestException("Stock quantity cannot be negative");
                }
                _context.BrandInventories.Update(inventory);
                await _context.SaveChangesAsync();
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
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

            if (existing == null)
                return false;

            var currentStock = existing.StockQuantity ?? 0;
            var newStock = currentStock + deltaQuantity;

            if (newStock < 0)
                return false;

            existing.StockQuantity = newStock;

            _context.BrandInventories.Update(existing);
            await _context.SaveChangesAsync();

            return true;
        }

        
    }
}
