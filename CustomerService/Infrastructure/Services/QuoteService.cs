using System.Text.Json;
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
    private readonly VehicleRepository _vehicleRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public QuoteService(
        QuoteRepository quoteRepository, 
        VehicleVersionRepository versionRepository, 
        VehicleRepository vehicleRepository, 
        CustomerRepository customerRepository,
        IMapper mapper)
    {
        _quoteRepository = quoteRepository;
        _vehicleVersionRepository = versionRepository;
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
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

        var optionsJson = "";

        if (!string.IsNullOrWhiteSpace(quote.OptionsJson))
        {
            var json = quote.OptionsJson.Trim();

            if (json.StartsWith("\""))
            {
                try
                {
                    json = JsonSerializer.Deserialize<string>(json) ?? json;
                }
                catch
                {
                    
                }
            }

            try
            {
                var deserialized = JsonSerializer.Deserialize<List<string>>(json);
                optionsJson = deserialized == null ? string.Empty : string.Join(", ", deserialized);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"OptionsJson parse error: {ex.Message}");
                optionsJson = string.Empty;
            }
        }


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
            OptionsJson = optionsJson,
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
    /*
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

        if (request.OptionsJson != null)
        {
            var optionsJson = request.OptionsJson
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            request.OptionsJson = JsonSerializer.Serialize(optionsJson);
        }

        _mapper.Map(request, quote);

        var discount = request.DiscountAmt ?? 0;
        quote.TotalPrice = quote.Subtotal - discount;

        return await _quoteRepository.UpdateQuote(quote);
    }
    */
    public async Task<bool> UpdateQuoteByQuoteId(Guid quoteId, QuoteUpdateRequest request)
    {
        var quote = await _quoteRepository.GetQuoteByQuoteId(quoteId);
        if (quote == null)
            throw new KeyNotFoundException("Quote not found");
        var vehicle = await _vehicleRepository.GetVehicleByBrandAndName(request.Brand.Trim(), request.ModelName.Trim());
        if (vehicle == null)
            throw new NotFoundException("Vehicle not found, please check again Brand and ModelName of vehicle");
        
        var version =
            await _vehicleVersionRepository.GetVehicleVersionByNameAndColorAndVehicleId(request.VersionName.Trim(),
                request.Color.Trim(), vehicle.VehicleId);
        if (version == null)
            throw new NotFoundException("Version not found, please check again version name and color");
        
        var customer = await _customerRepository.GetCustomerByNameAndPhone(request.CustomerName.Trim(), request.CustomerPhone.Trim());
        if(customer == null)
            throw new NotFoundException("Customer not found, please check again customer name and phone");

        if (request.OptionsJson != null)
        {
            var optionsJson = request.OptionsJson
                .Trim()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            request.OptionsJson = JsonSerializer.Serialize(optionsJson);
        }
        
        var allowedStatuses = new[] { "draft", "confirmed", "expired", "cancelled" };

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new InvalidOperationException("Status cannot be empty.");
        }

        if (!allowedStatuses.Contains(request.Status.ToLower()))
        {
            throw new InvalidOperationException("Invalid status value. Only accepted: draft, confirmed, expired, cancelled.");
        }

        var discount = request.DiscountAmt ?? 0;
        
        quote.CustomerId = customer.CustomerId;
        quote.VehicleVersionId = version.VehicleVersionId;
        quote.OptionsJson = request.OptionsJson;
        quote.DiscountAmt = discount;
        quote.Subtotal = version.BasePrice;
        quote.TotalPrice = quote.Subtotal - discount;
        quote.Status = request.Status;

        return await _quoteRepository.UpdateQuote(quote);
    }
}