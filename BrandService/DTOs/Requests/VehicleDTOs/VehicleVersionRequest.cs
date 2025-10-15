namespace BrandService.DTOs.Requests.VehicleDTOs
{
    public class VehicleVersionRequest
    {
        public string VersionName { get; set; } = null!;
        public string? Color { get; set; }
        public string? EvType { get; set; }
        public int? HorsePower { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
    }
}
