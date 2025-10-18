using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
using BrandService.Extensions.Query;
using BrandService.Model;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class VehicleVersionRepository
    {
        private readonly MyDbContext _context;
        public VehicleVersionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleVersion> AddVersionAsync(VehicleVersion version)
        {
            try
            {
                if (DoesVersionExist(version.VehicleId, version.VersionName, version.Color))
                    throw new BadRequestException("Vehicle version with the same name and color already exists for this vehicle.");

                _context.VehicleVersions.Add(version);
                await _context.SaveChangesAsync();
                return version;
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

        public async Task<VehicleVersion>? UpdateVersionAsync(VehicleVersion version)
        {
            try
            {
                var existingVersion = await _context.VehicleVersions
                    .FirstOrDefaultAsync(v => v.VehicleVersionId == version.VehicleVersionId);
                if (existingVersion == null)
                    throw new NotFoundException("Vehicle version not found");
                if (DoesVersionExist(version.VehicleId, version.VersionName, version.Color) &&
                    (existingVersion.VersionName.ToLower() != version.VersionName.ToLower() ||
                     existingVersion.Color.ToLower() != version.Color.ToLower()))
                {
                    throw new BadRequestException("Another vehicle version with the same name and color already exists for this vehicle.");
                }
                existingVersion.VersionName = version.VersionName;
                existingVersion.Color = version.Color;
                existingVersion.EvType = version.EvType;
                existingVersion.HorsePower = version.HorsePower;
                existingVersion.BasePrice = version.BasePrice;
                existingVersion.ImageUrl = version.ImageUrl;
                await _context.SaveChangesAsync();
                return existingVersion;
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

        public async Task<PagedResult<VehicleVersion>> GetPagedAsync(int pageNumber, int pageSize, string? searchValue)
        {
            try
            {
                var query = _context.VehicleVersions
                    .Include(vv => vv.Vehicle)
                    .Include(vv => vv.BrandInventory)
                    .Include(vv => vv.Inventories)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    var keyword = searchValue.Trim().ToLower();

                    query = query.Where(vv =>
                        vv.Vehicle.Brand.ToLower().Contains(keyword) ||
                        vv.Vehicle.ModelName.ToLower().Contains(keyword) ||
                        vv.VersionName.ToLower().Contains(keyword) ||
                        (vv.Color != null && vv.Color.ToLower().Contains(keyword)) ||
                        (vv.EvType != null && vv.EvType.ToLower().Contains(keyword))
                    );
                }

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderBy(vv => vv.Vehicle.Brand)
                    .ThenBy(vv => vv.Vehicle.ModelName)
                    .ThenBy(vv => vv.VersionName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<VehicleVersion>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vehicle versions: {ex.Message}");
            }
        }

        public async Task<PagedResult<VehicleVersion>> GetPagedByDealerAsync(Guid dealerId, int pageNumber, int pageSize, string? searchValue)
        {
            try
            {
                var dealerExists = await _context.Dealers
                    .AsNoTracking()
                    .AnyAsync(d => d.DealerId == dealerId);

                if (!dealerExists)
                    throw new NotFoundException("Dealer not found");

                var query = _context.VehicleVersions
                    .Include(vv => vv.Vehicle)
                    .Include(vv => vv.Inventories.Where(i => i.DealerId == dealerId))
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    searchValue = searchValue.Trim().ToLower();

                    query = query.Where(vv =>
                        vv.Vehicle.Brand.ToLower().Contains(searchValue) ||
                        vv.Vehicle.ModelName.ToLower().Contains(searchValue) ||
                        vv.VersionName.ToLower().Contains(searchValue) ||
                        (vv.Color != null && vv.Color.ToLower().Contains(searchValue))
                    );
                }

                query = query.OrderBy(vv => vv.Vehicle.ModelName).ThenBy(vv => vv.VersionName);

                return await query.ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when getting vehicle versions for dealer");
            }
        }


        public async Task<VehicleVersion?> GetVersionByIdAsync(Guid id)
        {
            try
            {
                var version = await _context.VehicleVersions
                                            .Include(vv => vv.Vehicle)
                                            .Include(vv => vv.Inventories)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(v => v.VehicleVersionId == id);

                if (version == null)
                    throw new NotFoundException("Vehicle version not found");

                return version;
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

        public async Task<bool> DeleteVersionAsync(Guid id)
        {
            try
            {
                var version = await _context.VehicleVersions.FindAsync(id);
                if (version == null)
                    throw new NotFoundException("Vehicle version not found");
                _context.VehicleVersions.Remove(version);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                throw new BadRequestException("Cannot delete vehicle version as it is referenced by other records.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DoesVersionExist(Guid vehicleId, string versionName, string color)
        {
            return _context.VehicleVersions
                .AsNoTracking()
                .Any(v =>
                    v.VehicleId == vehicleId &&
                    v.VersionName.ToLower() == versionName.ToLower() &&
                    v.Color.ToLower() == color.ToLower());
        }
    }
}
