using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs.Requests.OrderDTOs;

public class OrderCreateRequest
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public Guid ContractId { get; set; }
    
    [Required]
    public string DeliveryAddress { get; set; }
    
    [Required]
    public DateOnly DeliveryDate { get; set; }
}