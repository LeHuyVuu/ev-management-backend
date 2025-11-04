namespace BrandService.Kafka.Event;

public record ConfirmOrderEvent
{
    public Guid OrderId { get; set; }
    public string OrderType { get; set; } 
    public Guid? DealerId { get; set; }
    
    public string Status { get; set; }
    public bool IsApproved { get; set; }
    public string? Reason { get; set; }
}