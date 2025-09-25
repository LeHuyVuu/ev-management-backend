namespace ProductService.DTOs;

public class CustomerCreateModel
{
    public Guid DealerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Status { get; set; } = "Active";

    public DateOnly LastInteractionDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}