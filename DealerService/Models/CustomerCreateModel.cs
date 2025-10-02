namespace ProductService.DTOs;

public class CustomerCreateModel
{
    public Guid DealerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Status { get; set; } = "active";

    public DateOnly LastInteractionDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}