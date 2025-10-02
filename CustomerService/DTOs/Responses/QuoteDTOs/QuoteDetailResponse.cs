namespace CustomerService.DTOs.Responses.QuoteDTOs;

public class QuoteDetailResponse
{
    public Guid QuoteId {get; set;}
    
    public Guid DealerId { get; set; }
    
    public Guid CustomerId { get; set; }
    
    public Guid VehicleVersionId { get; set; }
    
    public string CustomerName {get; set;}
    
    public string CustomerPhone {get; set;} 
    
    public string Brand { get; set; } = null!;

    public string ModelName { get; set; } = null!;
    
    public string VersionName { get; set; } = null!;
    
    public string? Color { get; set; }
    
    public string? OptionsJson { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmt { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;
}