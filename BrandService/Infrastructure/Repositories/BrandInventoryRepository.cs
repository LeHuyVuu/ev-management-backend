using BrandService.Context;
using BrandService.Entities;
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

        public async Task UpdateInventoryAsync(BrandInventory inventory)
        {
            try
            {
                var existingInventory = await _context.BrandInventories
                    .FirstOrDefaultAsync(i => i.VehicleVersionId == inventory.VehicleVersionId);
                if (existingInventory == null)
                    throw new Exception("Inventory not found");
                existingInventory.StockQuantity = inventory.StockQuantity;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
