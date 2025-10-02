using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

public class DealerRepository
{
    private readonly MyDbContext _dbContext;

    public DealerRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dealer> GetDealerByDealerId(Guid dealerId)
    {
        return await _dbContext.Dealers.FirstOrDefaultAsync(d => d.DealerId == dealerId);
    }
}