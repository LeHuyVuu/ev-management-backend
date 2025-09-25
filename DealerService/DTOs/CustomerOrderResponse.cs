namespace ProductService.DTOs;

public class CustomerOrderResponse
{
    public Guid OrderId { get; set; }

    public Guid DealerId { get; set; }

    public Guid ContractId { get; set; }

    public Guid CustomerId { get; set; }

    public string? DeliveryAddress { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}