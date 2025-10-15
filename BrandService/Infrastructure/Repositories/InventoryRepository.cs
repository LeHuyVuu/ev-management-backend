using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class InventoryRepository
    {
        private readonly MyDbContext _context;

        public InventoryRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory>> GetAllAsync()
        {
            try
            {
                return await _context.Inventories
                    .AsNoTracking()
                    .OrderByDescending(i => i.LastUpdated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Inventory>> GetByDealerAsync(Guid dealerId)
        {
            try
            {
                return await _context.Inventories
                    .AsNoTracking()
                    .Where(i => i.DealerId == dealerId)
                    .OrderByDescending(i => i.LastUpdated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Inventory> UpdateStockAsync(Guid id, int stockQuantity)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    throw new NotFoundException("Inventory not found");

                inventory.StockQuantity = stockQuantity;
                inventory.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                await _context.SaveChangesAsync();

                return inventory;
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
