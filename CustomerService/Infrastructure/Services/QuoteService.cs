using Application.ExceptionHandler;
using AutoMapper;
using ProductService.DTOs.Requests.QuoteDTOs;
using ProductService.DTOs.Responses.QuoteDTOs;
using ProductService.Entities;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure.Services;

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
            DealerId = quote.DealerId,
            CustomerId = customer.CustomerId,
            VehicleVersionId = vehicleVersion.VehicleVersionId,
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

    public async Task<IEnumerable<QuoteBasicResponse>> GetQuotesByDealerId(Guid dealerId)
    {
        var quotes = await _quoteRepository.GetQuotesByDealerId(dealerId);

        var quoteResponses = quotes.Select(quote => new QuoteBasicResponse
        {
            QuoteId = quote.QuoteId,
            CustomerName = quote.Customer.Name,
            Phone = quote.Customer.Phone,
            Brand = quote.VehicleVersion.Vehicle.Brand,
            VehicleName = quote.VehicleVersion.Vehicle.ModelName,
            VersionName = quote.VehicleVersion.VersionName,
            TotalPrice = quote.TotalPrice,
        });
        return quoteResponses;
    }

        public async Task<bool> UpdateQuoteByQuoteId(Guid quoteId, QuoteUpdateRequest request)
        {
            var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
            if(quote == null) 
                throw new KeyNotFoundException("Quote not found");
            
            if (request.VehicleVersionId != quote.VehicleVersionId || request.VehicleVersionId != Guid.Empty)
            {
                var vehicleVersion = await _vehicleVersionRepository.GetVehicleVersionByVersionId(request.VehicleVersionId);
                if (vehicleVersion == null)
                    throw new KeyNotFoundException("Vehicle version not found");
                quote.Subtotal = vehicleVersion.BasePrice;
            }
            
            _mapper.Map(request, quote);
            
            quote.TotalPrice = quote.Subtotal - request.DiscountAmt ?? 0;

            return await _quoteRepository.UpdateQuote(quote);
        }
}