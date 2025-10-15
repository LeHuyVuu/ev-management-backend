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

        public async Task<PagedResult<Vehicle>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.VehicleVersions)
                    .AsNoTracking()
                    .OrderByDescending(v => v.ModelName)
                    .ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleVersions)
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
    }
}
