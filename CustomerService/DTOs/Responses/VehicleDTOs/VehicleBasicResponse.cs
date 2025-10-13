namespace CustomerService.DTOs.Responses.VehicleDTOs;

public class VehicleBasicResponse
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; }

    public string ModelName { get; set; }

    public string Description { get; set; }
}