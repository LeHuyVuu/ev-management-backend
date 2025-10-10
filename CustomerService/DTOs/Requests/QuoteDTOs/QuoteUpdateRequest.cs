using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs.Requests.QuoteDTOs;

public class QuoteUpdateRequest
{
    // public Guid QuoteId { get; set; }
        
    // public Guid DealerId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public string? OptionsJson { get; set; }

    public decimal? DiscountAmt { get; set; }

    public string? Status { get; set; }
}