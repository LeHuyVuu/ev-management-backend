using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure.Controller;

public class QuoteController
{
    private readonly QuoteService _quoteService;

    public QuoteController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }
}