using BrandService.DTOs.Requests.VehicleDTOs;
using BrandService.DTOs.Responses.VehicleDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService _vehicleService;

        public VehicleController(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // ---------------- VEHICLES ----------------

        /// <summary>
        /// Get all vehicles with pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<VehicleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var vehicles = await _vehicleService.GetPagedAsync(pageNumber, pageSize);
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
        /// Create a new vehicle model.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] CreateVehicleRequest request)
        {
            var created = await _vehicleService.AddAsync(request);
            return Ok(ApiResponse<VehicleResponse>.Success(created.Data));
        }

        // ---------------- VEHICLE VERSIONS ----------------

        /// <summary>
        /// Add a new version for a vehicle.
        /// </summary>
        [HttpPost("{vehicleId}/versions")]
        [ProducesResponseType(typeof(ApiResponse<VehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddVersion(Guid vehicleId, [FromBody] CreateVehicleVersionRequest request)
        {
            var version = await _vehicleService.AddVersionAsync(vehicleId, request);
            return Ok(ApiResponse<VehicleVersionResponse>.Success(version.Data));
        }

        /// <summary>
        /// Get details of a specific vehicle version.
        /// </summary>
        [HttpGet("versions/{versionId}")]
        [ProducesResponseType(typeof(ApiResponse<VehicleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetVersion(Guid versionId)
        {
            var version = await _vehicleService.GetVersionByIdAsync(versionId);
            return Ok(ApiResponse<VehicleVersionResponse>.Success(version.Data));
        }
    }
}
