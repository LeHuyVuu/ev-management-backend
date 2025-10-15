using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs.Requests.QuoteDTOs;

public class QuoteUpdateRequest
{
    // public Guid CustomerId { get; set; }

   // public Guid VehicleVersionId { get; set; }
   
   public string CustomerName { get; set; }
   
   public string CustomerPhone {  get; set; }
   
   public string Brand {  get; set; }
   
   public string ModelName { get; set; }
   
   public string VersionName { get; set; }
   
   public string Color { get; set; }

    public string? OptionsJson { get; set; }

    public decimal? DiscountAmt { get; set; }

    public string? Status { get; set; }
}