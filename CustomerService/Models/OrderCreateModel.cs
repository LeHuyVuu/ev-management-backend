namespace CustomerService.Models;

public class OrderCreateModel
{
    public Guid DealerId { get; set; }
    public Guid ContractId { get; set; }
    public Guid CustomerId { get; set; }
    public string DeliveryAddress { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public string Status { get; set; } = "preparing";
}