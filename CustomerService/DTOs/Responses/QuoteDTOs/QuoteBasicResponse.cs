namespace CustomerService.DTOs.Responses.QuoteDTOs;

public class QuoteBasicResponse
{
    public Guid QuoteId {get; set;}
    
    public string CustomerName {get; set;}
    
    public string Phone { get; set; }
    
    public string Brand {get; set;}
    
    public string VehicleName {get; set;}
    
    public string VersionName {get; set;}
    
    public decimal TotalPrice {get; set;}
}