using Microsoft.AspNetCore.Mvc;
using UtilityService.Infrastructure.Services;
using UtilityService.Models;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _svc;

    public ForecastController(ForecastService svc)
    {
        _svc = svc;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ForecastRow>>> Search(
        [FromQuery] Guid? dealerId,
        [FromQuery] Guid? vehicleVersionId,
        [FromQuery] DateTime? fromWeek,
        [FromQuery] DateTime? toWeek,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var q = new ForecastQuery
        {
            DealerId = dealerId,
            VehicleVersionId = vehicleVersionId,
            FromWeek = fromWeek,
            ToWeek = toWeek,
            Page = page,
            PageSize = pageSize
        };

        var result = await _svc.SearchAsync(q, ct);
        return Ok(result);
    }

    [HttpPost("ask")]
    public async Task<ActionResult<object>> Ask([FromBody] AskRequestDto req, CancellationToken ct)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("question is required");

        var answer = await _svc.AskAsync(req.Question, req.Forecast, ct);
        return Ok(new { answer });
    }
}