namespace CustomerService.DTOs.Requests.OrderDTOs;

public class OrderUpdateRequest
{
    public string? DeliveryAddress { get; set; }
    public DateOnly? DeliveryDate { get; set; }
    public string? Note { get; set; } // optional client note (not persisted unless column exists)
}
