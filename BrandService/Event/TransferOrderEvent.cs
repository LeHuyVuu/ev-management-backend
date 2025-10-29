namespace BrandService.Event;

public record TransferOrderEvent
{
    public Guid? DealerId { get; set; }
    public Guid? VehicleVersionId { get; set; }
    public int? Quantity { get; set; }
    public bool IsRecived { get; set; }
}