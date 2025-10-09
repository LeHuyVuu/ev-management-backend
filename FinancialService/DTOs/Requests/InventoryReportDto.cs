namespace FinancialService.Dtos
{
    public class InventoryReportDto
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public DateOnly LastUpdated { get; set; }
    }
}