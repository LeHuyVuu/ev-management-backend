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

    public Task<Customer?> GetCustomerByQuoteId(Guid quoteId)
    {
        return _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.Customer)
            .FirstOrDefaultAsync();
    }

    public Task<Vehicle?> GetVehicleByQuoteId(Guid quoteId)
    {
        return _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.VehicleVersion.Vehicle)
            .FirstOrDefaultAsync();
    }

    public Task<VehicleVersion?> GetVehicleVersionByQuoteId(Guid quoteId)
    {
        return _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.VehicleVersion)
            .FirstOrDefaultAsync();
    }

    public Task<Quote?> GetQuoteByQuoteId(Guid quoteId)
    {
        return _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .FirstOrDefaultAsync();
    }
}