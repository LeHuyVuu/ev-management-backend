using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
using BrandService.Model;
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

        public async Task<Inventory> GetByIdAsync(Guid id)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Dealer)
                    .Include(i => i.VehicleVersion)
                        .ThenInclude(vv => vv.Vehicle)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InventoryId == id);
                if (inventory == null)
                    throw new NotFoundException("Inventory not found");
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

        public async Task<PagedResult<Inventory>> GetPagedAsync(int pageNumber, int pageSize, string? searchValue)
        {
            try
            {
                var query = _context.Inventories
                    .Include(i => i.Dealer)
                    .Include(i => i.VehicleVersion)
                        .ThenInclude(vv => vv.Vehicle)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    var keyword = searchValue.Trim().ToLower();
                    query = query.Where(i =>
                        i.Dealer.Name.ToLower().Contains(keyword) ||
                        i.Dealer.DealerCode.ToLower().Contains(keyword) ||
                        i.VehicleVersion.Vehicle.ModelName.ToLower().Contains(keyword) ||
                        i.VehicleVersion.VersionName.ToLower().Contains(keyword) ||
                        (i.VehicleVersion.Color != null && i.VehicleVersion.Color.ToLower().Contains(keyword))
                    );
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderBy(i => i.Dealer.Name)
                    .ThenBy(i => i.VehicleVersion.Vehicle.ModelName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Inventory>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching inventories: {ex.Message}");
            }
        }

        public async Task<PagedResult<Inventory>> GetByDealerAsync(Guid dealerId, int pageNumber, int pageSize, string? searchValue)
        {
            try
            {
                var query = _context.Inventories
                    .Include(i => i.Dealer)
                    .Include(i => i.VehicleVersion)
                        .ThenInclude(vv => vv.Vehicle)
                    .Where(i => i.DealerId == dealerId)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    var keyword = searchValue.Trim().ToLower();
                    query = query.Where(i =>
                        i.VehicleVersion.Vehicle.ModelName.ToLower().Contains(keyword) ||
                        i.VehicleVersion.VersionName.ToLower().Contains(keyword) ||
                        (i.VehicleVersion.Color != null && i.VehicleVersion.Color.ToLower().Contains(keyword))
                    );
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderBy(i => i.VehicleVersion.Vehicle.ModelName)
                    .ThenBy(i => i.VehicleVersion.VersionName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Inventory>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching dealer inventories: {ex.Message}");
            }
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

        public async Task<Inventory> UpdateDealerStockAsync(Guid dealerId, Guid versionId, int deltaQuantity)
        {
            try
            {
                var existing = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.DealerId == dealerId && i.VehicleVersionId == versionId);
                if (existing == null)
                {
                    throw new NotFoundException("Inventory not found");
                }
                var newStock = existing.StockQuantity + deltaQuantity;
                if (newStock < 0)
                {
                    throw new BadRequestException("Cannot decrease stock by more than the current available quantity");
                }
                existing.StockQuantity = newStock;
                existing.LastUpdated = DateOnly.FromDateTime(DateTime.UtcNow);
                _context.Inventories.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
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
