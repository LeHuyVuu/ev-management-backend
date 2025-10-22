namespace BrandService.DTOs.Responses.VehicleDTOs
{
    public class DealerVehicleVersionResponse
    {
        public Guid VehicleVersionId { get; set; }
        public Guid VehicleId { get; set; }
        public string Brand { get; set; } = null!;
        public string ModelName { get; set; } = null!;
        public string VersionName { get; set; } = null!;
        public string? Color { get; set; }
        public string? EvType { get; set; }
        public int? HorsePower { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }      // tồn kho của đại lý
    }
}
