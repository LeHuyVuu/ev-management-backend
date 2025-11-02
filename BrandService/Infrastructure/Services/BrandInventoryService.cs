using BrandService.Entities;
using BrandService.Infrastructure.Repositories;

namespace BrandService.Infrastructure.Services
{
    public class BrandInventoryService
    {
        private readonly BrandInventoryRepository _brandInventoryRepo;
        public BrandInventoryService(BrandInventoryRepository brandInventoryRepo)
        {
            _brandInventoryRepo = brandInventoryRepo;
        }

        public async Task UpdateStockAsync(Guid vehicleVersionId, int deltaQuantity)
        {
            await _brandInventoryRepo.UpdateStockAsync(vehicleVersionId, deltaQuantity);
        }
    }
}
