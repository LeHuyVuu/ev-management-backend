using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs.Requests.ContractDTOs;

public class ContractCreateRequest
{
    [Required] 
    public Guid QuoteId { get; set; }

    [Required] 
    public Guid CustomerId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;
}