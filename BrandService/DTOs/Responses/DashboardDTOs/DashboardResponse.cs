namespace BrandService.DTOs.Responses.DashboardDTOs
{
    public class DashboardResponse
    {
        public int TotalVersions { get; set; }
        public int TotalInventory { get; set; }
        public int TotalDealers { get; set; }
        public int PendingDistributionOrders { get; set; }
    }
}
