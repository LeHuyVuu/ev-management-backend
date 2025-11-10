using Microsoft.AspNetCore.Mvc;
using UtilityService.Infrastructure.Services;

namespace UtilityService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _svc;

    public ForecastController(ForecastService svc)
    {
        _svc = svc;
    }

    // GET /api/forecast/weekly?dealerId=...&vehicleVersionId=...&horizon=8
    [HttpGet("weekly")]
    public async Task<IActionResult> Weekly(
        [FromQuery] Guid dealerId,
        [FromQuery] Guid vehicleVersionId,
        [FromQuery] int horizon = 8)
    {
        try
        {
            var fc = await _svc.ForecastWeekly(dealerId, vehicleVersionId, horizon);
            var res = fc.Select(x => new { week_start = x.WeekStart.ToString("yyyy-MM-dd"), demand = Math.Round(x.Forecast) });
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/forecast/weekly-total?vehicleVersionId=...&horizon=8
    [HttpGet("weekly-total")]
    public async Task<IActionResult> WeeklyTotal(
        [FromQuery] Guid vehicleVersionId,
        [FromQuery] int horizon = 8)
    {
        try
        {
            var res = await _svc.ForecastWeeklyTotal(vehicleVersionId, horizon);
            var shaped = res.Select(x => new { week_start = x.WeekStart.ToString("yyyy-MM-dd"), demand = Math.Round(x.ForecastTotal) });
            return Ok(shaped);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/forecast/allocate?vehicleVersionId=...&horizon=4&holdback=0.1
    [HttpGet("allocate")]
    public async Task<IActionResult> Allocate(
        [FromQuery] Guid vehicleVersionId,
        [FromQuery] int horizon = 4,
        [FromQuery] double holdback = 0.10)
    {
        try
        {
            var plan = await _svc.AllocateByForecast(vehicleVersionId, horizon, holdback);
            var shaped = plan.Select(p => new
            {
                dealer_id = p.DealerId,
                vehicle_version_id = p.VehicleVersionId,
                quantity = p.Quantity
            });
            return Ok(shaped);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/forecast/debug-all?page=1&pageSize=20&horizon=8&search=&dealerId=&vehicleVersionId=
    [HttpGet("debug-all")]
    public async Task<IActionResult> DebugAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int horizon = 8,
        [FromQuery] string? search = null,
        [FromQuery] Guid? dealerId = null,
        [FromQuery] Guid? vehicleVersionId = null)
    {
        try
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var res = await _svc.GetAllMlDebugPagedAsync(
                page: page,
                pageSize: pageSize,
                horizon: horizon,
                search: search,
                dealerId: dealerId,
                vehicleVersionId: vehicleVersionId
            );

            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
