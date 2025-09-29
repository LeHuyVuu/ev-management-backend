using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Entities;

namespace ProductService.Infrastructure.Repositories;

public class QuoteRepository
{
    private readonly MyDbContext _dbContext;

    public QuoteRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetCustomerByQuoteId(Guid quoteId)
    {
        return await _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.Customer)
            .FirstOrDefaultAsync();
    }

    public async Task<Vehicle?> GetVehicleByQuoteId(Guid quoteId)
    {
        return await _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.VehicleVersion.Vehicle)
            .FirstOrDefaultAsync();
    }
}