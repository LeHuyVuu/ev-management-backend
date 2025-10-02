using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

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
}