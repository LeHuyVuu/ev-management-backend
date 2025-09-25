namespace ProductService.DTOs;

public class OrderCustomerResponse
{
    public Guid OrderId { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}