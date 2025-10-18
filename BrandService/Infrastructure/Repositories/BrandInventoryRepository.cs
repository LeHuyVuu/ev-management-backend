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

        public async Task UpdateStockAsync(Guid vehicleVersionId, int deltaQuantity)
        {
            try
            {
                var existing = await _context.BrandInventories.FirstOrDefaultAsync(bi => bi.VehicleVersionId == vehicleVersionId);
                if (existing == null)
                {
                    throw new NotFoundException("Brand inventory not found");
                }
                var newStock = (existing.StockQuantity ?? 0) + deltaQuantity;
                if (newStock < 0)
                {
                    throw new BadRequestException("Cannot decrease stock by more than the current available quantity");
                }
                existing.StockQuantity = newStock;
                _context.BrandInventories.Update(existing);
                await _context.SaveChangesAsync();
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
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

        
    }
}
