using CustomerService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Infrastructure.Controller;

public class VehicleVersionController : ControllerBase
{
    private readonly VehicleVersionService _vehicleVersionService;

    public VehicleVersionController(VehicleVersionService vehicleVersionService)
    {
        _vehicleVersionService = vehicleVersionService;
    }

    /// <summary>
    /// Lấy ra tất cả các phiên bản của xe mà mình muốn xem
    /// </summary>
    [HttpGet]
    [Route("/vehicles/{id}/versions")]
    public async Task<IActionResult> GetVehicleVersionsByVehicleId(Guid id)
    {
        return Ok(await _vehicleVersionService.GetVehicleVersionsByVehicleID(id));
    }
}