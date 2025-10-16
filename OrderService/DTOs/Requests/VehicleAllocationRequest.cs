namespace IntelliAIService.DTOs.Requests;

public class VehicleAllocationRequest
{

    public Guid DealerId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public int Quantity { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime? ExpectedDelivery { get; set; }

    public string Status { get; set; } = null!;
}