namespace BrandService.DTOs.Responses.ReportDTOs
{
    public class SalesReportResponse
    {
        public SalesReportFilter Filter { get; set; } = new();
        public List<SalesByModelRegion> SalesSummary { get; set; } = new();
    }

    public class SalesReportFilter
    {
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? Year { get; set; }
        public string? Region { get; set; }
        public Guid? DealerId { get; set; }
    }

    public class SalesByModelRegion
    {
        public string Model { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
