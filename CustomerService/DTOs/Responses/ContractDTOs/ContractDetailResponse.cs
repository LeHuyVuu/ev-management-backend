namespace CustomerService.DTOs.Responses.ContractDTOs;

public class ContractDetailResponse
{
    public Guid ContractId { get; set; }
    
    public string CustomerName { get; set; }
    
    public string CustomerPhone { get; set; }
    
    public string CustomerEmail { get; set; }
    
    public string DealerName { get; set; }
    
    public string DealerPhone {  get; set; }
    
    public string DealerEmail { get; set; }
    
    public string Brand {get; set; }
    
    public string VehicleName {get; set; }
    
    public string VersionName {get; set; }
    
    public decimal TotalValue { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly SignedDate { get; set; }
}