using BrandService.DTOs.Responses.ReportDTOs;
using BrandService.Infrastructure.Repositories;

namespace BrandService.Infrastructure.Services
{
    public class SalesReportService
    {
        private readonly SalesReportRepository _repo;

        public SalesReportService(SalesReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<SalesReportResponse> GetSalesReportAsync(int? month, int? quarter, int? year, string? region, Guid? dealerId)
        {
            var data = await _repo.GetSalesByModelAndRegionAsync(month, quarter, year, region, dealerId);

            return new SalesReportResponse
            {
                Filter = new SalesReportFilter
                {
                    Month = month,
                    Quarter = quarter,
                    Year = year,
                    Region = region,
                    DealerId = dealerId
                },
                SalesSummary = data
            };
        }
    }
}
