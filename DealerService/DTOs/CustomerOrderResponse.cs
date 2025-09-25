namespace ProductService.DTOs;

public class CustomerOrderResponse
{
    public Guid OrderId { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}