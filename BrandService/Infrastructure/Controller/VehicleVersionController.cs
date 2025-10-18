using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/vehicle-versions")]
    public class VehicleVersionController : ControllerBase
    {
        private readonly VehicleVersionService _vehicleVersionService;
        public VehicleVersionController(VehicleVersionService vehicleVersionService)
        {
            _vehicleVersionService = vehicleVersionService;
        }

        /// <summary>
        /// Get a paginated list of vehicle versions with optional search.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BrandVehicleVersionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var result = await _vehicleVersionService.GetPagedAsync(pageNumber, pageSize, searchValue);
            return Ok(result);
        }

        /// <summary>
        /// Get a paginated list of vehicle versions for the authenticated dealer with optional search.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpGet("dealer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerVehicleVersionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByDealerId([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var dealerIdClaim = User.FindFirst("DealerId")?.Value;

            if (dealerIdClaim == null)
                return Unauthorized(ApiResponse<object>.Fail(403, "Dealer information missing in token."));

            var dealerId = Guid.Parse(dealerIdClaim);
            var result = await _vehicleVersionService.GetPagedForDealerAsync(dealerId, pageNumber, pageSize, searchValue);
            return Ok(result);
        }

        /// <summary>
        /// Add a new version for a vehicle.
        /// </summary>
        [HttpPost("{vehicleId}")]
        [ProducesResponseType(typeof(ApiResponse<BrandVehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddVersion(Guid vehicleId, [FromBody] VehicleVersionRequest request)
        {
            var version = await _vehicleVersionService.AddVersionAsync(vehicleId, request);
            return Ok(ApiResponse<BrandVehicleVersionResponse>.Success(version.Data));
        }

        /// <summary>
        /// Get details of a specific vehicle version.
        /// </summary>
        [HttpGet("{versionId}")]
        [ProducesResponseType(typeof(ApiResponse<BrandVehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetVersion(Guid versionId)
        {
            var version = await _vehicleVersionService.GetVersionByIdAsync(versionId);
            return Ok(ApiResponse<BrandVehicleVersionResponse>.Success(version.Data));
        }

        /// <summary>
        /// Update an existing vehicle version.
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{versionId}")]
        [ProducesResponseType(typeof(ApiResponse<BrandVehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateVersion(Guid versionId, [FromBody] VehicleVersionRequest request)
        {
            var version = await _vehicleVersionService.UpdateVersionAsync(versionId, request);
            return Ok(ApiResponse<BrandVehicleVersionResponse>.Success(version.Data));
        }

        /// <summary>
        /// Delete a vehicle version with reference check.
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        [HttpDelete("{versionId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteVersion(Guid versionId)
        {
            var result = await _vehicleVersionService.DeleteVersionAsync(versionId);
            return Ok(result);
        }
    }
}
