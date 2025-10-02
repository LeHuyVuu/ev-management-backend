using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

public class DealerRepository
{
    private readonly MyDbContext _myDbContext;

    public DealerRepository(MyDbContext myDbContext)
    {
        _myDbContext = myDbContext;
    }

    public async Task<Dealer> GetDealerByDealerId(Guid dealerId)
    {
        return await _myDbContext.Dealers.FirstOrDefaultAsync(d => d.DealerId == dealerId);
    }
}