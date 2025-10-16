using BrandService.DTOs.Requests.InventoryDTOs;
using BrandService.DTOs.Responses.InventoryDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/inventories")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get inventory details by inventoryId.
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        [HttpGet("{inventoryId}")]
        [ProducesResponseType(typeof(ApiResponse<InventoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(Guid inventoryId)
        {
            var inventory = await _inventoryService.GetByIdAsync(inventoryId);
            return Ok(inventory);
        }

        /// <summary>
        /// Get a paginated list of inventory items for all dealers with optional search.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var result = await _inventoryService.GetPagedAsync(pageNumber, pageSize, searchValue);
            return Ok(result);
        }

        /// <summary>
        /// Get inventory items for a specific dealer with optional search.
        /// </summary>
        /// <param name="dealerId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpGet("dealer/{dealerId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var result = await _inventoryService.GetByDealerAsync(dealerId, pageNumber, pageSize, searchValue);
            return Ok(result);
        }

        ///// <summary>
        ///// Update inventory stock quantity.
        ///// </summary>
        //[HttpPut("{inventoryId}")]
        //[ProducesResponseType(typeof(ApiResponse<InventoryResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> Update(Guid id, [FromBody] UpdateInventoryRequest request)
        //{
        //    var updated = await _inventoryService.UpdateStockAsync(id, request);
        //    return Ok(ApiResponse<InventoryResponse>.Success(updated.Data, "Stock quantity updated successfully."));
        //}
    }
}
