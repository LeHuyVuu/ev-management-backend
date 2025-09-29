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
        var customerTask  = _quoteRepository.GetCustomerByQuoteId(quoteId);
        var vehicleTask  = _quoteRepository.GetVehicleByQuoteId(quoteId);
        var versionTask  = _quoteRepository.GetVehicleVersionByQuoteId(quoteId);
        var quoteTask  = _quoteRepository.GetQuoteByQuoteId(quoteId);

        await Task.WhenAll(customerTask, vehicleTask, versionTask, quoteTask);
        
        var customer = customerTask.Result;
        var vehicle = vehicleTask.Result;
        var version = versionTask.Result;
        var quote = quoteTask.Result;

        if (quote != null && (customer == null || vehicle == null || version == null))
        {
            throw new NotFoundException("Đã có 1 trong 3 object (customer, vehicle, version không tồn tại");
        }
        
        return new QuoteDetailResponse()
        {
            QuoteId = quote.QuoteId,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            Brand = vehicle.Brand,
            ModelName = vehicle.ModelName,
            VersionName = version.VersionName,
            Color = version.Color,
            OptionsJson = quote.OptionsJson,
            DiscountCode = quote.DiscountCode,
            Subtotal = quote.Subtotal,
            DiscountAmt = quote.DiscountAmt,
            TotalPrice = quote.TotalPrice,
            Status = quote.Status,
        };
    }
}