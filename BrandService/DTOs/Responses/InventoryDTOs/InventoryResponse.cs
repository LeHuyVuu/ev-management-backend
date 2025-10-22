using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;

namespace BrandService.DTOs.Responses.InventoryDTOs
{
    public class InventoryResponse
    {
        public Guid InventoryId { get; set; }
        public DealerResponse Dealer { get; set; } = null!;
        public DealerVehicleVersionResponse VehicleVersion { get; set; } = null!;
        public int StockQuantity { get; set; }
        public DateOnly LastUpdated { get; set; }
    }
}
