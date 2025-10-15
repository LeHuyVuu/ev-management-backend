using IntelliAIService.DTOs.Requests;
using IntelliAIService.DTOs.Responses;
using IntelliAIService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using OrderService.Model;

namespace IntelliAIService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class VehicleAllocationController : ControllerBase
{
    private readonly VehicleAllocationService _service;
    private readonly ILogger<VehicleAllocationController> _logger;

    public VehicleAllocationController(
        VehicleAllocationService service,
        ILogger<VehicleAllocationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ✅ 1️⃣ GET /api/vehicle-allocations?dealerId=xxx&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetVehicleAllocations(
        [FromQuery] Guid? dealerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetVehicleAllocationsAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleAllocationResponse>>.Success(result, "Fetched vehicle allocations successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle allocations");
            return StatusCode(500, ApiResponse<PagedResult<VehicleAllocationResponse>>.Fail(
                500, "Internal server error", ex.Message));
        }
    }

    // ✅ 2️⃣ POST /api/vehicle-allocations
    [HttpPost]
    public async Task<IActionResult> CreateVehicleAllocation([FromBody] VehicleAllocationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<VehicleAllocationResponse>.Fail(400, "Invalid request", ModelState));

            var created = await _service.CreateAsync(request);
            return StatusCode(201, ApiResponse<VehicleAllocationResponse>.Success(created, "Vehicle allocation created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle allocation");
            return StatusCode(500, ApiResponse<VehicleAllocationResponse>.Fail(
                500, "Internal server error", ex.Message));
        }
    }

    // ✅ 3️⃣ PUT /api/vehicle-allocations/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(ApiResponse<string>.Fail(400, "Status cannot be empty."));

            var updated = await _service.UpdateStatusAsync(id, status);
            if (updated == null)
                return NotFound(ApiResponse<string>.Fail(404, "Vehicle allocation not found."));

            return Ok(ApiResponse<VehicleAllocationResponse>.Success(updated, "Vehicle allocation status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle allocation status");
            return StatusCode(500, ApiResponse<VehicleAllocationResponse>.Fail(
                500, "Internal server error", ex.Message));
        }
    }
}
