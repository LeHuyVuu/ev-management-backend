namespace BrandService.DTOs.Responses.ReportDTOs
{
    public class CurrentInventoryResponse
    {
        public string Model { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public int DealersStock { get; set; }
        public DateOnly? LastUpdated { get; set; }
    }
}
