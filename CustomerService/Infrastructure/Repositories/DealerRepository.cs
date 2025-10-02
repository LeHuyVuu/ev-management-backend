using Microsoft.EntityFrameworkCore;
using CustomerService.Context;
using CustomerService.Entities;

namespace CustomerService.Infrastructure.Repositories;

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