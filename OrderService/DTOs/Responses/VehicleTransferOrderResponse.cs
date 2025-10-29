namespace OrderService.DTOs.Responses;

public class VehicleTransferOrderResponse
{
    public Guid VehicleTransferOrderId { get; set; }
    
    public Guid? FromDealerId { get; set; }
    public string FromDealerName { get; set; }   

    public Guid? ToDealerId { get; set; }

    public string ToDealerName { get; set; }

    public string VehicleName { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? RequestDate { get; set; }

    public string? Status { get; set; }
}