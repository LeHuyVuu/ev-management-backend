using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService _vehicleService;
        public VehicleController(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Get a paginated list of vehicles with optional search.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<VehicleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchValue = null)
        {
            var vehicles = await _vehicleService.GetPagedAsync(pageNumber, pageSize, searchValue);
            return Ok(ApiResponse<PagedResult<VehicleResponse>>.Success(vehicles.Data));
        }

        /// <summary>
        /// Get vehicle details by Id (includes all versions).
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<VehicleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(Guid id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            return Ok(ApiResponse<VehicleDetailResponse>.Success(vehicle.Data));
        }

        /// <summary>
        /// Create a new vehicle model with duplicate check.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] VehicleRequest request)
        {
            var created = await _vehicleService.AddAsync(request);
            return Ok(ApiResponse<VehicleResponse>.Success(created.Data));
        }

        /// <summary>
        /// Update an existing vehicle model by Id with duplicate check.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] VehicleRequest request)
        {
            var updated = await _vehicleService.UpdateAsync(id, request);
            return Ok(ApiResponse<VehicleResponse>.Success(updated.Data));
        }

        /// <summary>
        /// Delete a vehicle model by Id with reference check.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleted = await _vehicleService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Success(deleted.Data));
        }
    }
}
