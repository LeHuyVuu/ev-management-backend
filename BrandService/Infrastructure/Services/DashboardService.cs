using BrandService.DTOs.Responses.DashboardDTOs;
using BrandService.Infrastructure.Repositories;

namespace BrandService.Infrastructure.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _dashboardRepo;

        public DashboardService(DashboardRepository dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public async Task<DashboardResponse> GetDashboardAsync()
        {
            var totalVersions = await _dashboardRepo.GetTotalVehicleVersionsAsync();
            var totalStock = await _dashboardRepo.GetTotalStockQuantityAsync();
            var totalDealers = await _dashboardRepo.GetTotalActiveDealersAsync();
            var pendingTransferOrders = await _dashboardRepo.GetPendingTransferOrdersAsync();
            var pendingAllocationOrders = await _dashboardRepo.GetPendingAllocationsAsync();

            return new DashboardResponse
            {
                TotalVersions = totalVersions,
                TotalInventory = totalStock,
                TotalDealers = totalDealers,
                PendingDistributionOrders = pendingTransferOrders + pendingAllocationOrders
            };
        }
    }
}
