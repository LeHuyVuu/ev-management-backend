namespace BrandService.Kafka.Event;

public class AllocationOrderEvent
{
    public Guid OrderId { get; set; }
    public Guid? DealerId { get; set; }
    public Guid? VehicleVersionId { get; set; }
    
    public string Status { get; set; }
    public int? Quantity { get; set; }
    public bool IsRecived { get; set; }
}