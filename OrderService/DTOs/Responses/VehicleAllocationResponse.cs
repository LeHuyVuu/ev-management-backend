namespace IntelliAIService.DTOs.Responses;

public class VehicleAllocationResponse
{
    public Guid AllocationId { get; set; }

    public string DealerName { get; set; }

    public string VehicleName { get; set; }

    public int Quantity { get; set; }

    public DateOnly RequestDate { get; set; }

    public DateOnly? ExpectedDelivery { get; set; }

    public string Status { get; set; } = null!;
}