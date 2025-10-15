using BrandService.Context;
using BrandService.Entities;
using BrandService.ExceptionHandler;
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
