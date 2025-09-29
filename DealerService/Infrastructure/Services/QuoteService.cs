using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class QuoteService
{
    private readonly QuoteRepository _quoteRepository;

    public QuoteService(QuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }
}