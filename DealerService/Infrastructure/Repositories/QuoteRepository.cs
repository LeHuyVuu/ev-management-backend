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

    public async Task<VehicleVersion?> GetVehicleVersionByQuoteId(Guid quoteId)
    {
        return await _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .Select(q => q.VehicleVersion)
            .FirstOrDefaultAsync();
    }

    public async Task<Quote?> GetQuoteByQuoteId(Guid quoteId)
    {
        return await _dbContext.Quotes
            .Where(q => q.QuoteId == quoteId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Quote>> GetQuotesByDealerId(Guid dealerId)
    {
        return await _dbContext.Quotes
            .Where(q => q.DealerId == dealerId)
            .Include(q => q.Customer)
            .Include(q => q.VehicleVersion.Vehicle)
            .ToListAsync();
    }

    public async Task<bool> UpdateQuote(Quote quote)
    {
        _dbContext.Quotes.Update(quote);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}