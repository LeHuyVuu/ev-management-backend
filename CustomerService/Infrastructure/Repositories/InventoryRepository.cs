using CustomerService.Context;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public class InventoryRepository
{
    private readonly MyDbContext _dbContext;

    public InventoryRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Inventory> GetInventoryByVersionId(Guid versionId)
    {
        return await _dbContext.Inventories.Where(i => i.VehicleVersionId == versionId).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateInventory(Inventory inventory)
    {
        _dbContext.Update(inventory);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}