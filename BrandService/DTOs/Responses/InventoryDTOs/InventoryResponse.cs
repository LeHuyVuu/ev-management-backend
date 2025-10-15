namespace BrandService.DTOs.Responses.InventoryDTOs
{
    public class InventoryResponse
    {
        public Guid InventoryId { get; set; }
        public Guid DealerId { get; set; }
        public Guid VehicleVersionId { get; set; }
        public int StockQuantity { get; set; }
        public DateOnly LastUpdated { get; set; }
    }
}
