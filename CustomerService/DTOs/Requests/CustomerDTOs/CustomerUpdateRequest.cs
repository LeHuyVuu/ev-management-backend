using System.ComponentModel.DataAnnotations;

namespace CustomerService.DTOs.Requests.CustomerDTOs;

public class CustomerUpdateRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, Phone]
    public string Phone { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;
    
    [Required]
    public string Status { get; set; } = null!;
}