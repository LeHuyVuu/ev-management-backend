using CustomerService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Infrastructure.Controller;

[ApiController]
public class VehicleController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public VehicleController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    [Route("/vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        return Ok(await _vehicleService.GetVehicles());
    }
}