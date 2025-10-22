namespace BrandService.DTOs.Responses.VehicleDTOs
{
    public class VehicleResponse
    {
        public Guid VehicleId { get; set; }
        public string Brand { get; set; } = null!;
        public string ModelName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
