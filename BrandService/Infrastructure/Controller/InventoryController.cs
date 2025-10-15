using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get all inventory across all dealers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<InventoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll()
        {
            var inventories = await _inventoryService.GetAllAsync();
            return Ok(ApiResponse<List<InventoryResponse>>.Success(inventories.Data));
        }

        /// <summary>
        /// Get inventory by dealer Id.
        /// </summary>
        [HttpGet("{dealerId}")]
        [ProducesResponseType(typeof(ApiResponse<List<InventoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByDealer(Guid dealerId)
        {
            var dealerInventories = await _inventoryService.GetByDealerAsync(dealerId);
            return Ok(ApiResponse<List<InventoryResponse>>.Success(dealerInventories.Data));
        }

        /// <summary>
        /// Update inventory stock quantity.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateInventoryRequest request)
        {
            var updated = await _inventoryService.UpdateStockAsync(id, request);
            return Ok(ApiResponse<InventoryResponse>.Success(updated.Data, "Stock quantity updated successfully."));
        }
    }
}
