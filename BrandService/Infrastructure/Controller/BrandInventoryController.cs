using BrandService.Entities;
using BrandService.Infrastructure.Services;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/brand-inventories")]
    public class BrandInventoryController : ControllerBase
    {
        private readonly BrandInventoryService _brandInventoryService;

        public BrandInventoryController(BrandInventoryService brandInventoryService)
        {
            _brandInventoryService = brandInventoryService;
        }

        /// <summary>
        /// Update increment or decrement stock quantity for a vehicle version.
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="deltaQuantity"></param>
        /// <returns></returns>
        [HttpPatch("{versionId}/update-stock")]
        [ProducesResponseType(typeof(ApiResponse<BrandInventory>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateStock(Guid versionId, [FromQuery] int deltaQuantity)
        {
            // deltaQuantity > 0 => tăng, < 0 => giảm
            await _brandInventoryService.UpdateStockAsync(versionId, deltaQuantity);
            return Ok(ApiResponse<object>.Success(null, "Stock updated successfully"));
        }
    }
}
