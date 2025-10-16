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

        public async Task<BrandInventory> UpdateStockAsync(Guid vehicleVersionId, int deltaQuantity)
        {
            try
            {
                var inventory = await _context.BrandInventories
               .FirstOrDefaultAsync(b => b.VehicleVersionId == vehicleVersionId);

                if (inventory == null)
                    throw new NotFoundException("Brand inventory not found for this vehicle version.");

                var newStock = inventory.StockQuantity + deltaQuantity;
                if (newStock < 0)
                    throw new BadRequestException("Stock quantity cannot be negative.");

                inventory.StockQuantity = newStock;

                _context.BrandInventories.Update(inventory);
                await _context.SaveChangesAsync();

                return inventory;
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

        public async Task<BrandInventory> UpdateInventoryAsync(Guid versionId, int stockQuantity)
        {
            try
            {
                if (stockQuantity < 0)
                {
                    throw new BadRequestException("Stock quantity cannot be negative");
                }
                var existing = await _context.BrandInventories.FirstOrDefaultAsync(bi => bi.VehicleVersionId == versionId);
                if (existing == null)
                {
                    throw new NotFoundException("Brand inventory not found");
                }
                existing.StockQuantity = stockQuantity;
                _context.BrandInventories.Update(existing);
                await _context.SaveChangesAsync();
                return existing;

            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
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
