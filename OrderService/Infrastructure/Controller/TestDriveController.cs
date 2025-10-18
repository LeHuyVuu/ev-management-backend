using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Infrastructure.Services;
using OrderService.Model;

namespace OrderService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDriveController : ControllerBase
    {
        private readonly TestDriveService _service;
        private readonly ILogger<TestDriveController> _logger;

        public TestDriveController(
            TestDriveService service,
            ILogger<TestDriveController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ✅ 1️⃣ GET /api/test-drives?dealerId=xxx&pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetTestDrives(
            [FromQuery] Guid? dealerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetTestDrivesAsync(dealerId, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResult<TestDriveResponse>>.Success(result, "Fetched test drives successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test drives");
                return StatusCode(500, ApiResponse<PagedResult<TestDriveResponse>>.Fail(
                    500, "Internal server error", ex.Message));
            }
        }

        // ✅ 2️⃣ GET /api/test-drives/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(ApiResponse<string>.Fail(404, "Test drive not found."));

                return Ok(ApiResponse<TestDriveResponse>.Success(result, "Fetched test drive successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test drive with ID {Id}", id);
                return StatusCode(500, ApiResponse<TestDriveResponse>.Fail(
                    500, "Internal server error", ex.Message));
            }
        }

        // ✅ 3️⃣ POST /api/test-drives?dealerId=xxx
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TestDriveRequest request, [FromQuery] Guid dealerId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<TestDriveResponse>.Fail(400, "Invalid request", ModelState));

                var created = await _service.CreateAsync(request, dealerId);
                if (created == null)
                    return StatusCode(500, ApiResponse<TestDriveResponse>.Fail(500, "Failed to create test drive."));

                return StatusCode(201, ApiResponse<TestDriveResponse>.Success(created, "Test drive created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test drive for dealer {DealerId}", dealerId);
                return StatusCode(500, ApiResponse<TestDriveResponse>.Fail(
                    500, "Internal server error", ex.Message));
            }
        }

        // ✅ 4️⃣ PATCH /api/test-drives/{id}/status
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    return BadRequest(ApiResponse<string>.Fail(400, "Status cannot be empty."));

                var updated = await _service.UpdateStatusAsync(id, status);
                if (updated == null)
                    return NotFound(ApiResponse<string>.Fail(404, "Test drive not found."));

                return Ok(ApiResponse<TestDriveResponse>.Success(updated, "Test drive status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating test drive status for ID {Id}", id);
                return StatusCode(500, ApiResponse<TestDriveResponse>.Fail(
                    500, "Internal server error", ex.Message));
            }
        }
    }
}
