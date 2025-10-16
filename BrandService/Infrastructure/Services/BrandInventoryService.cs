using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Models;

namespace BrandService.Infrastructure.Services
{
    public class BrandInventoryService
    {
        private readonly BrandInventoryRepository _brandInventoryRepo;
        public BrandInventoryService(BrandInventoryRepository brandInventoryRepo)
        {
            _brandInventoryRepo = brandInventoryRepo;
        }

        public async Task<ApiResponse<BrandInventory>> UpdateStockAsync(Guid vehicleVersionId, int deltaQuantity)
        {
            var updated = await _brandInventoryRepo.UpdateStockAsync(vehicleVersionId, deltaQuantity);
            return ApiResponse<BrandInventory>.Success(updated, "Stock updated successfully");
          
        }
    }
}
