namespace BrandService.DTOs.Requests.VehicleDTOs
{
    public class VehicleRequest
    {
        public string Brand { get; set; } = null!;
        public string ModelName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
