namespace ProductService.DTOs;

public class ContractDealerResponse
{
    public Guid ContractId { get; set; }
    
    public string CustomerName { get; set; }
    
    public string CustomerPhone {  get; set; }
    
    public string Brand {get; set; }
    
    public string VehicleName {get; set; }
    
    public string VersionName {get; set; }
    
    public decimal TotalValue { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly SignedDate { get; set; }
}