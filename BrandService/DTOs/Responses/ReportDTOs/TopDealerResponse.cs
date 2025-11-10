namespace BrandService.DTOs.Responses.ReportDTOs
{
    public class TopDealerResponse
    {
        public string DealerName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public string GrowthPercent { get; set; } = "0%";
    }

    public class TopDealersReportResponse
    {
        public List<TopDealerResponse> TopDealers { get; set; } = new();
    }
}
