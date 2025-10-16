using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
using BrandService.Extensions.Query;
using BrandService.Model;
using Microsoft.EntityFrameworkCore;

namespace BrandService.Infrastructure.Repositories
{
    public class VehicleRepository
    {
        private readonly MyDbContext _context;

        public VehicleRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Vehicle>> GetPagedAsync(int pageNumber, int pageSize, string? searchValue)
        {
            try
            {
                var query = _context.Vehicles
                    .Include(v => v.VehicleVersions)
                        .ThenInclude(vv => vv.BrandInventory) 
                    .Include(v => v.VehicleVersions)
                        .ThenInclude(vv => vv.Inventories) 
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    var keyword = searchValue.Trim().ToLower();
                    query = query.Where(v =>
                        v.Brand.ToLower().Contains(keyword) ||
                        v.ModelName.ToLower().Contains(keyword) ||
                        (v.Description != null && v.Description.ToLower().Contains(keyword))
                    );
                }

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(v => v.ModelName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Vehicle>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching paged vehicles: {ex.Message}");
            }
        }


        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleVersions).ThenInclude(vv => vv.BrandInventory)
                    .Include(v => v.VehicleVersions).ThenInclude(vv => vv.Inventories)
                    .FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                    throw new NotFoundException("Vehicle not found");

                return vehicle;
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

        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            try
            {
                if (DoesVehicleExist(vehicle.Brand, vehicle.ModelName))
                    throw new BadRequestException("Vehicle with the same brand and model name already exists.");

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                return vehicle;
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

        public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
        {
            try
            {
                var existingVehicle = await _context.Vehicles.FindAsync(vehicle.VehicleId);
                if (existingVehicle == null)
                    throw new NotFoundException("Vehicle not found");
                if (DoesVehicleExist(vehicle.Brand, vehicle.ModelName) &&
                    (existingVehicle.Brand.ToLower() != vehicle.Brand.ToLower() ||
                     existingVehicle.ModelName.ToLower() != vehicle.ModelName.ToLower()))
                {
                    throw new BadRequestException("Another vehicle with the same brand and model name already exists.");
                }
                existingVehicle.Brand = vehicle.Brand;
                existingVehicle.ModelName = vehicle.ModelName;
                existingVehicle.Description = vehicle.Description;
                existingVehicle.VehicleVersions = vehicle.VehicleVersions;
                _context.Vehicles.Update(existingVehicle);
                await _context.SaveChangesAsync();
                return existingVehicle;
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

        public bool DoesVehicleExist(string brand, string modelName)
        {
            return _context.Vehicles
                .AsNoTracking()
                .Any(v => v.Brand.ToLower() == brand.ToLower() && v.ModelName.ToLower() == modelName.ToLower());
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    throw new NotFoundException("Vehicle not found");
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                throw new BadRequestException("Cannot delete vehicle because it is referenced by other vehicle versions");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
