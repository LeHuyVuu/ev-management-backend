using Application.ExceptionHandler;
using ProductService.DTOs.Responses.QuoteDTOs;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

public class QuoteService
{
    private readonly QuoteRepository _quoteRepository;

    public QuoteService(QuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public async Task<QuoteDetailResponse> GetQuoteDetailByQuoteId(Guid quoteId)
    {
        var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
        if (quote == null)
            throw new NotFoundException($"Quote not found");

        var customer = await _quoteRepository.GetCustomerByQuoteId(quoteId);
        if (customer == null)
            throw new NotFoundException($"Customer not found");

        var vehicle = await _quoteRepository.GetVehicleByQuoteId(quoteId);
        if (vehicle == null)
            throw new NotFoundException($"Vehicle not found");

        var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quoteId);
        if (vehicleVersion == null)
            throw new NotFoundException($"Vehicle not found");
        
        return new QuoteDetailResponse()
        {
            QuoteId = quote.QuoteId,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            Brand = vehicle.Brand,
            ModelName = vehicle.ModelName,
            VersionName = vehicleVersion.VersionName,
            Color = vehicleVersion.Color,
            OptionsJson = quote.OptionsJson,
            Subtotal = quote.Subtotal,
            DiscountAmt = quote.DiscountAmt,
            TotalPrice = quote.TotalPrice,
            Status = quote.Status,
        };
    }
}