using ProductService.Context;

namespace ProductService.Infrastructure.Repositories;

public class QuoteRepository
{
    private readonly MyDbContext _dbContext;

    public QuoteRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
}