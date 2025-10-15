namespace CustomerService.DTOs.Responses.VehicleVersionDTOs;

public class VersionBasicResponse
{
    public string VersionName { get; set; }

    public string? Color { get; set; }

    public string? EvType { get; set; }

    public int? HorsePower { get; set; }
}