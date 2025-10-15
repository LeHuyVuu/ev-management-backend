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

        public async Task<VehicleVersion> AddVersionAsync(VehicleVersion version)
        {
            try
            {
                _context.VehicleVersions.Add(version);
                await _context.SaveChangesAsync();
                return version;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<VehicleVersion?> GetVersionByIdAsync(Guid id)
        {
            try
            {
                var version = await _context.VehicleVersions
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

        public bool DoesVehicleExist(string brand, string modelName)
        {
            return _context.Vehicles
                .AsNoTracking()
                .Any(v => v.Brand.ToLower() == brand.ToLower() && v.ModelName.ToLower() == modelName.ToLower());
        }
    }
}
