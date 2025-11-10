namespace CustomerService.DTOs.Responses.QuoteDTOs;

using System.Collections.Generic;

public class QuoteDetailResponse
{
    public Guid QuoteId {get; set;}
    
    public Guid DealerId { get; set; }

    public string DealerName { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName {get; set; }

    public string CustomerPhone {get; set;} 
    
    public string Brand { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public Guid VehicleVersionId { get; set; }

    public string VersionName { get; set; } = null!;
    
    public string? Color { get; set; }
    
    // Return promotions as a list of string IDs (parsed from comma-separated storage)
    public List<string>? PromotionNames { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmt { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;
}