namespace ProductService.DTOs;

public class ContractCustomerResponse
{
    public Guid ContractId { get; set; }
    
    public string Brand {get; set; }
    
    public string VehicleName {get; set; }
    
    public string VersionName {get; set; }
    
    public decimal TotalValue { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly SignedDate { get; set; }
}