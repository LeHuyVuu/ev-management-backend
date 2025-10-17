using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs.Requests;
using OrderService.DTOs.Responses;
using OrderService.Infrastructure.Services;
using OrderService.Model;

namespace OrderService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class VehicleTransferOrderController : ControllerBase
{
    private readonly VehicleTransferOrderService _service;
    private readonly ILogger<VehicleTransferOrderController> _logger;

    public VehicleTransferOrderController(
        VehicleTransferOrderService service,
        ILogger<VehicleTransferOrderController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ✅ 1️⃣ GET /api/vehicle-transfer-orders?fromDealerId=xxx&toDealerId=yyy&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetVehicleTransferOrders(
        [FromQuery] Guid? fromDealerId,
        [FromQuery] Guid? toDealerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetVehicleTransferOrdersAsync(fromDealerId, toDealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleTransferOrderResponse>>.Success(
                result, "Fetched vehicle transfer orders successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle transfer orders");
            return StatusCode(500, ApiResponse<PagedResult<VehicleTransferOrderResponse>>.Fail(
                500, "Internal server error", ex.Message));
        }
    }

    // ✅ 2️⃣ POST /api/vehicle-transfer-orders
    [HttpPost]
    public async Task<IActionResult> CreateVehicleTransferOrder([FromBody] VehicleTransferOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<VehicleTransferOrderResponse>.Fail(400, "Invalid request", ModelState));

            var created = await _service.CreateAsync(request);
            return StatusCode(201, ApiResponse<VehicleTransferOrderResponse>.Success(
                created, "Vehicle transfer order created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle transfer order");
            return StatusCode(500, ApiResponse<VehicleTransferOrderResponse>.Fail(
                500, "Internal server error", ex.Message));
        }
    }

    // ✅ 3️⃣ PUT /api/vehicle-transfer-orders/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(ApiResponse<string>.Fail(400, "Status cannot be empty."));

            var updated = await _service.UpdateStatusAsync(id, status);
            if (updated == null)
                return NotFound(ApiResponse<string>.Fail(404, "Vehicle transfer order not found."));

            return Ok(ApiResponse<VehicleTransferOrderResponse>.Success(
                updated, "Vehicle transfer order status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle transfer order status");
            return StatusCode(500, ApiResponse<VehicleTransferOrderResponse>.Fail(
                500, "Internal server error", ex.Message));
        }
    }
}
