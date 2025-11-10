namespace OrderService.DTOs.Responses;

public class VehicleAllocationDetailResponse
{
    public Guid AllocationId { get; set; }
    public Guid DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public Guid VehicleVersionId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public DateOnly RequestDate { get; set; }
    public DateOnly? ExpectedDelivery { get; set; }
    public string Status { get; set; } = string.Empty;
}
