using FinancialService.Model;
using FinancialService.Repositories;

namespace FinancialService.Services
{
    public class InventoryReportService
    {
        private readonly InventoryReportRepository _repo;

        public InventoryReportService(InventoryReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<object>> GetInventoryReportAsync(Guid? dealerId, string? sortBy, string? order)
        {
            try
            {
                var result = await _repo.GetInventoryReportAsync(dealerId, sortBy, order);
                return ApiResponse<object>.Success(result, "Inventory report retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail(500, "Error retrieving inventory report", ex.Message);
            }
        }
    }
}