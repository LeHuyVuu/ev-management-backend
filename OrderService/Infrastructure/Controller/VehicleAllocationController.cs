using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Infrastructure.Services;
using OrderService.Model;

namespace OrderService.Infrastructure.Controller;

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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetVehicleAllocations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var roleId = User.FindFirstValue("RoleId");
            Guid? dealerId = null;

            if (roleId != "1" && roleId != "2")
            {
                var dealerIdClaim = User.FindFirstValue("DealerId");
                if (string.IsNullOrEmpty(dealerIdClaim) || !Guid.TryParse(dealerIdClaim, out var parsedDealerId))
                {
                    return Unauthorized(ApiResponse<TestDriveResponse>.Fail(401, "Invalid or missing DealerId in token"));
                }
                dealerId = parsedDealerId;
            }

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

    /// <summary>
    /// Lấy ra chi tiết phân bổ xe
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        try
        {
            var detail = await _service.GetDetailAsync(id);
            if (detail == null)
                return NotFound(ApiResponse<string>.Fail(404, "Vehicle allocation not found."));

            return Ok(ApiResponse<VehicleAllocationDetailResponse>.Success(detail, "Fetched vehicle allocation detail successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle allocation detail for {Id}", id);
            return StatusCode(500, ApiResponse<VehicleAllocationDetailResponse>.Fail(
                500, "Internal server error", ex.Message));
        }
    }

    // ✅ 2️⃣ POST /api/vehicle-allocations
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateVehicleAllocation([FromBody] VehicleAllocationRequest request)
    {
        var dealerIdClaim = User.FindFirstValue("DealerId");

        if (string.IsNullOrEmpty(dealerIdClaim) || !Guid.TryParse(dealerIdClaim, out var dealerId))
        {
            return Unauthorized(ApiResponse<TestDriveResponse>.Fail(401, "Invalid or missing DealerId in token"));
        }
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<VehicleAllocationResponse>.Fail(400, "Invalid request", ModelState));
            
            var created = await _service.CreateAsync(request, dealerId);
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
