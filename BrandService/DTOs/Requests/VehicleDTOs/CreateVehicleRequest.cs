namespace BrandService.DTOs.Requests.VehicleDTOs
{
    public class CreateVehicleRequest
    {
        public string Brand { get; set; } = null!;
        public string ModelName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
