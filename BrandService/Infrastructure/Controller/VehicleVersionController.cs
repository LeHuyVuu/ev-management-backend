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
        [ProducesResponseType(typeof(ApiResponse<PagedResult<VehicleVersionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var result = await _vehicleVersionService.GetPagedAsync(pageNumber, pageSize, searchValue);
            return Ok(result);
        }

        /// <summary>
        /// Add a new version for a vehicle.
        /// </summary>
        [HttpPost("{vehicleId}")]
        [ProducesResponseType(typeof(ApiResponse<VehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddVersion(Guid vehicleId, [FromBody] VehicleVersionRequest request)
        {
            var version = await _vehicleVersionService.AddVersionAsync(vehicleId, request);
            return Ok(ApiResponse<VehicleVersionResponse>.Success(version.Data));
        }

        ///// <summary>
        ///// Get details of a specific vehicle version.
        ///// </summary>
        //[HttpGet("{versionId}")]
        //[ProducesResponseType(typeof(ApiResponse<BrandVehicleVersionResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> GetVersion(Guid versionId)
        //{
        //    var version = await _vehicleVersionService.GetVersionByIdAsync(versionId);
        //    return Ok(ApiResponse<BrandVehicleVersionResponse>.Success(version.Data));
        //}

        //[HttpGet("{dealerId}")]
        //[ProducesResponseType(typeof(ApiResponse<IEnumerable<BrandVehicleVersionResponse>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> GetVersionsByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string? searchValue)
        //{
        //    var versions = await _vehicleVersionService.GetVersionsByDealerIdAsync(dealerId);
        //    return Ok(ApiResponse<IEnumerable<BrandVehicleVersionResponse>>.Success(versions.Data));
        //}


        /// <summary>
        /// Update an existing vehicle version.
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{versionId}")]
        [ProducesResponseType(typeof(ApiResponse<VehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateVersion(Guid versionId, [FromBody] VehicleVersionRequest request)
        {
            var version = await _vehicleVersionService.UpdateVersionAsync(versionId, request);
            return Ok(ApiResponse<VehicleVersionResponse>.Success(version.Data));
        }

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
