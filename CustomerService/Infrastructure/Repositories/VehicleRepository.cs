using CustomerService.Context;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public class VehicleRepository
{
    private readonly MyDbContext _dbContext;

    public VehicleRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Vehicle> GetVehicleByBrandAndName(string brand, string name)
    {
        return await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Brand == brand && v.ModelName == name);
    }

    public async Task<IEnumerable<Vehicle>> GetVehicles()
    {
        return await _dbContext.Vehicles.ToListAsync();
    }
}