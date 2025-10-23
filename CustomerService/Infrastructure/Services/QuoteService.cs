using System.Text.Json;
using CustomerService.ExceptionHandler;
using AutoMapper;
using CustomerService.DTOs.Requests.QuoteDTOs;
using CustomerService.DTOs.Responses.QuoteDTOs;
using CustomerService.Entities;
using CustomerService.Infrastructure.Repositories;
using System.Security.Claims;

namespace CustomerService.Infrastructure.Services;

public class QuoteService
{
    private readonly QuoteRepository _quoteRepository;
    private readonly VehicleVersionRepository _vehicleVersionRepository;
    private readonly VehicleRepository _vehicleRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly PromotionRepository _promotionRepository;
    private readonly DealerRepository _dealerRepository;
    private readonly UserRepository _userRepository;
    private readonly IMapper _mapper;

    public QuoteService(
        QuoteRepository quoteRepository,
        VehicleVersionRepository versionRepository,
        VehicleRepository vehicleRepository,
        CustomerRepository customerRepository,
        PromotionRepository promotionRepository,
        IMapper mapper,
        DealerRepository dealerRepository,
        UserRepository userRepository)
    {
        _quoteRepository = quoteRepository;
        _vehicleVersionRepository = versionRepository;
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
        _promotionRepository = promotionRepository;
        _mapper = mapper;
        _dealerRepository = dealerRepository;
        _userRepository = userRepository;
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
            OptionsJson = ParsePromotionIds(quote.OptionsJson),
            Subtotal = quote.Subtotal,
            DiscountAmt = quote.DiscountAmt,
            TotalPrice = quote.TotalPrice,
            Status = string.IsNullOrWhiteSpace(quote.Status) ? "draft" : quote.Status.ToLowerInvariant()
        };
    }

    public async Task<IEnumerable<QuoteBasicResponse>> GetQuotesByDealerId(Guid dealerId)
    {
        var quotes = await _quoteRepository.GetQuotesByDealerId(dealerId);

        if (quotes == null || !quotes.Any())
        {
            return Enumerable.Empty<QuoteBasicResponse>();
        }

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

        var vehicle = await _vehicleRepository.GetVehicleByBrandAndName(request.Brand.Trim(), request.ModelName.Trim());
        if (vehicle == null)
            throw new NotFoundException("Vehicle not found, please check again Brand and ModelName of vehicle");

        var version = await _vehicleVersionRepository
            .GetVehicleVersionByNameAndColorAndVehicleId(request.VersionName.Trim(), request.Color.Trim(), vehicle.VehicleId);
        if (version == null)
            throw new NotFoundException("Version not found, please check again version name and color");

        var customer = await _customerRepository.GetCustomerByNameAndPhone(request.CustomerName.Trim(), request.CustomerPhone.Trim());
        if (customer == null)
            throw new NotFoundException("Customer not found, please check again customer name and phone");

        var allowedStatuses = new[] { "draft", "confirmed", "expired", "cancelled" };

        if (string.IsNullOrWhiteSpace(request.Status))
            throw new InvalidOperationException("Status cannot be empty.");

        if (!allowedStatuses.Contains(request.Status.ToLower()))
            throw new InvalidOperationException("Invalid status value. Only accepted: draft, confirmed, expired, cancelled.");

        string optionsValue = null;
        if (request.PromotionIds is { Count: > 0 })
        {
            var promotions = await _promotionRepository.GetPromotionById(request.PromotionIds);
            if (promotions.Count() != request.PromotionIds.Count)
                throw new KeyNotFoundException("One or more promotions not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            foreach (var promoObj in promotions)
            {
                var promo = promoObj as Promotion;
                if (promo == null)
                    throw new InvalidOperationException("Promotion object is not of type Promotion.");

                if (!promo.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Promotion {promo.Name} is not active.");
                if (today < promo.StartDate || today > promo.EndDate)
                    throw new InvalidOperationException($"Promotion {promo.Name} is not in valid date range.");
            }
            optionsValue = string.Join(",", request.PromotionIds);
        }

        var discount = request.DiscountAmt ?? 0;
        quote.CustomerId = customer.CustomerId;
        quote.VehicleVersionId = version.VehicleVersionId;
        quote.OptionsJson = optionsValue;
        quote.DiscountAmt = discount;
        quote.Subtotal = version.BasePrice;
        quote.TotalPrice = quote.Subtotal - discount;
        quote.Status = request.Status;

        var result = await _quoteRepository.UpdateQuote(quote);
        return result;
    }

    public async Task<QuoteDetailResponse> CreateAsync(QuoteCreateRequest request, Guid dealerId, Guid createdByUser)
    {
        // Validate basic
        var allowedStatuses = new[] { "draft", "confirmed", "expired", "cancelled" };
        var status = string.IsNullOrWhiteSpace(request.Status) ? "draft" : request.Status.Trim().ToLowerInvariant();
        if (!allowedStatuses.Contains(status))
            throw new InvalidOperationException("Invalid status value. Only accepted: draft, confirmed, expired, cancelled.");

        // Find vehicle by brand + model
        var vehicle = await _vehicleRepository.GetVehicleByBrandAndName(request.Brand.Trim(), request.ModelName.Trim());
        if (vehicle == null)
            throw new NotFoundException("Vehicle not found, please check again Brand and ModelName of vehicle");

        // Find version by name + color + vehicleId
        var version = await _vehicleVersionRepository
            .GetVehicleVersionByNameAndColorAndVehicleId(request.VersionName.Trim(), request.Color.Trim(), vehicle.VehicleId);
        if (version == null)
            throw new NotFoundException("Version not found, please check again version name and color");

        // Find customer by name + phone
        var customer = await _customerRepository.GetCustomerByNameAndPhone(request.CustomerName.Trim(), request.CustomerPhone.Trim());
        if (customer == null)
            throw new NotFoundException("Customer not found, please check again customer name and phone");

        // Validate promotions
        string? optionsValue = null;
        if (request.PromotionIds is { Count: > 0 })
        {
            var promotions = await _promotionRepository.GetPromotionById(request.PromotionIds);
            if (promotions.Count() != request.PromotionIds.Count)
                throw new KeyNotFoundException("One or more promotions not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            foreach (var promoObj in promotions)
            {
                var promo = promoObj as Promotion;
                if (promo == null)
                    throw new InvalidOperationException("Promotion object is not of type Promotion.");
                if (!promo.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Promotion {promo.Name} is not active.");
                if (today < promo.StartDate || today > promo.EndDate)
                    throw new InvalidOperationException($"Promotion {promo.Name} is not in valid date range.");
            }
            optionsValue = string.Join(",", request.PromotionIds);
        }

        var discount = request.DiscountAmt ?? 0;

        var quote = new Quote
        {
            QuoteId = Guid.NewGuid(),
            DealerId = dealerId,
            CustomerId = customer.CustomerId,
            VehicleVersionId = version.VehicleVersionId,
            OptionsJson = optionsValue,
            DiscountAmt = discount,
            Subtotal = version.BasePrice,
            TotalPrice = version.BasePrice - discount,
            Status = status,
            CreatedByUser = createdByUser
        };

        // Persist
        await _quoteRepository.AddAsync(quote);

        // Build detail response
        return new QuoteDetailResponse
        {
            QuoteId = quote.QuoteId,
            DealerId = quote.DealerId,
            CustomerId = quote.CustomerId,
            VehicleVersionId = quote.VehicleVersionId,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Brand = request.Brand,
            ModelName = request.ModelName,
            VersionName = request.VersionName,
            Color = request.Color,
            OptionsJson = ParsePromotionIds(quote.OptionsJson),
            Subtotal = quote.Subtotal,
            DiscountAmt = quote.DiscountAmt,
            TotalPrice = quote.TotalPrice,
            Status = quote.Status
        };
    }

    private static List<string>? ParsePromotionIds(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
            return null;

        return optionsJson
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0)
            .ToList();
    }
}