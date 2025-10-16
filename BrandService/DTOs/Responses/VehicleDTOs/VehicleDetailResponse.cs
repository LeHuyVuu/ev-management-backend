using System.Collections.Generic;

namespace BrandService.DTOs.Responses.VehicleDTOs
{
    public class VehicleDetailResponse
    {
        public Guid VehicleId { get; set; }
        public string Brand { get; set; } = null!;
        public string ModelName { get; set; } = null!;
        public string? Description { get; set; }
        public List<VehicleVersionResponse> Versions { get; set; } = new();
    }
}
