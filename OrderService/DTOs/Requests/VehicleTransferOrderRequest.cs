namespace IntelliAIService.DTOs.Requests;

public class VehicleTransferOrderRequest
{
    public Guid? FromDealerId { get; set; }

    public Guid? ToDealerId { get; set; }

    public Guid? VehicleVersionId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? RequestDate { get; set; }

    public string? Status { get; set; }
}