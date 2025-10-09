using CustomerService.ExceptionHandler;
using AutoMapper;
using CustomerService.DTOs.Requests.QuoteDTOs;
using CustomerService.DTOs.Responses.QuoteDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;

namespace CustomerService.Infrastructure.Services;

public class QuoteService
{
    private readonly QuoteRepository _quoteRepository;
    private readonly VehicleVersionRepository _vehicleVersionRepository;
    private readonly IMapper _mapper;

    public QuoteService(QuoteRepository quoteRepository, VehicleVersionRepository versionRepository, IMapper mapper)
    {
        _quoteRepository = quoteRepository;
        _vehicleVersionRepository = versionRepository;
        _mapper = mapper;
    }

    public async Task<QuoteDetailResponse> GetQuoteDetailByQuoteId(Guid quoteId)
    {
        var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
        if (quote == null)
            throw new NotFoundException("Quote not found");

        var customer = await _quoteRepository.GetCustomerByQuoteId(quoteId);
        if (customer == null)
            throw new NotFoundException("Customer not found");

        var vehicle = await _quoteRepository.GetVehicleByQuoteId(quoteId);
        if (vehicle == null)
            throw new NotFoundException("Vehicle not found");

        var vehicleVersion = await _quoteRepository.GetVehicleVersionByQuoteId(quoteId);
        if (vehicleVersion == null)
            throw new NotFoundException("Vehicle version not found");

        return new QuoteDetailResponse
        {
            QuoteId = quote.QuoteId,
            DealerId = quote.DealerId,
            CustomerId = customer.CustomerId,
            VehicleVersionId = vehicleVersion.VehicleVersionId,
            CustomerName = customer.Name ?? "Unknown",
            CustomerPhone = customer.Phone ?? "Unknown",
            Brand = vehicle.Brand ?? "Unknown",
            ModelName = vehicle.ModelName ?? "Unknown",
            VersionName = vehicleVersion.VersionName ?? "Unknown",
            Color = vehicleVersion.Color ?? "Unknown",
            OptionsJson = quote.OptionsJson,
            Subtotal = quote.Subtotal,
            DiscountAmt = quote.DiscountAmt,
            TotalPrice = quote.TotalPrice,
            Status = quote.Status
        };
    }
    public async Task<IEnumerable<QuoteBasicResponse>> GetQuotesByDealerId(Guid dealerId)
    {
        var quotes = await _quoteRepository.GetQuotesByDealerId(dealerId);
        if (quotes == null || !quotes.Any())
            return Enumerable.Empty<QuoteBasicResponse>();

        var quoteResponses = quotes.Select(quote => new QuoteBasicResponse
        {
            QuoteId = quote.QuoteId,
            CustomerName = quote.Customer?.Name ?? "Unknown",
            Phone = quote.Customer?.Phone ?? "Unknown",
            Brand = quote.VehicleVersion?.Vehicle?.Brand ?? "Unknown",
            VehicleName = quote.VehicleVersion?.Vehicle?.ModelName ?? "Unknown",
            VersionName = quote.VehicleVersion?.VersionName ?? "Unknown",
            TotalPrice = quote.TotalPrice
        });

        return quoteResponses;
    }
    
    public async Task<bool> UpdateQuoteByQuoteId(Guid quoteId, QuoteUpdateRequest request)
    {
        var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
        if (quote == null)
            throw new KeyNotFoundException("Quote not found");

        // Update vehicle version if changed
        if (request.VehicleVersionId != Guid.Empty && request.VehicleVersionId != quote.VehicleVersionId)
        {
            var vehicleVersion = await _vehicleVersionRepository.GetVehicleVersionByVersionId(request.VehicleVersionId);
            if (vehicleVersion == null)
                throw new KeyNotFoundException("Vehicle version not found");

            quote.VehicleVersionId = vehicleVersion.VehicleVersionId;
            quote.Subtotal = vehicleVersion.BasePrice;
        }

        _mapper.Map(request, quote);

        var discount = request.DiscountAmt ?? 0;
        quote.TotalPrice = quote.Subtotal - discount;

        return await _quoteRepository.UpdateQuote(quote);
    }
}