namespace ProductService.DTOs.Responses.QuoteDTOs;

public class QuoteDetailResponse
{
    public Guid QuoteId {get; set;}
    
    public string CustomerName {get; set;}
    
    public string CustomerPhone {get; set;}
    
    public string VehicleBrand {get; set;}
    
    public string VehicleModelName {get; set;}
    
    public string VehicleVersionName {get; set;}
    
    public string VersionColor {get; set;}
    
    public string PromotionName {get; set;}
    
    public decimal WholesalePrice1 { get; set; }
    
    public decimal DiscountRate { get; set; }

    public decimal FinalPrice => WholesalePrice1 - (WholesalePrice1 * DiscountRate);

}