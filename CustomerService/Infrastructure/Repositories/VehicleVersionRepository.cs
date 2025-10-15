using Microsoft.EntityFrameworkCore;
using CustomerService.Context;
using CustomerService.Entities;

namespace CustomerService.Infrastructure.Repositories;

public class VehicleVersionRepository
{
    private readonly MyDbContext _dbContext;

    public VehicleVersionRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VehicleVersion> GetVehicleVersionByVersionId(Guid versionId)
    {
        return await _dbContext.VehicleVersions.FirstOrDefaultAsync(vv => vv.VehicleVersionId == versionId);
    }

    public async Task<VehicleVersion> GetVehicleVersionByNameAndColorAndVehicleId(string name, string color, Guid vehicleId)
    {
        return await _dbContext.VehicleVersions.FirstOrDefaultAsync(vv => vv.VersionName == name && vv.Color == color &&  vv.VehicleId == vehicleId);
    }

    public async Task<VehicleVersion> GetVersionByVehicleId(Guid vehicleId)
    {
        return await _dbContext.VehicleVersions.FirstOrDefaultAsync(vv => vv.VehicleId == vehicleId);
    }

    public async Task<IEnumerable<VehicleVersion>> GetVehicleVersionsByVehicleId(Guid vehicleId)
    {
        return await _dbContext.VehicleVersions.Where(vv => vv.VehicleId == vehicleId).ToListAsync();
    }
}