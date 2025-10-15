namespace BrandService.DTOs.Responses.VehicleDTOs
{
    public class VehicleVersionResponse
    {
        public Guid VehicleVersionId { get; set; }
        public Guid VehicleId { get; set; }
        public string VersionName { get; set; } = null!;
        public string? Color { get; set; }
        public string? EvType { get; set; }
        public int? HorsePower { get; set; }
        public decimal BasePrice { get; set; }
    }
}
