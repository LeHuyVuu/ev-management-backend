using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class CustomerCreateRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required,  EmailAddress]
    public string Email { get; set; } = null!;

    [Required,  Phone]
    public string Phone { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;
}