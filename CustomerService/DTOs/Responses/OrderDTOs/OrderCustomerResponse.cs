namespace CustomerService.DTOs.Responses.OrderDTOs;

public class OrderCustomerResponse
{
    public Guid OrderId { get; set; }

    public DateOnly? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;
}