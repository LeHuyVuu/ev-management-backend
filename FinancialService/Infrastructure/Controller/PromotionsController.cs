using FinancialService.Entities;
using FinancialService.Model;
using FinancialService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : ControllerBase
{
    private readonly PromotionService _service;

    public PromotionController(PromotionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams p)
    {
        var res = await _service.GetPagedAsync(p);
        return StatusCode(res.Status, res);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var res = await _service.GetByIdAsync(id);
        return StatusCode(res.Status, res);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
    {
        var res = await _service.CreateAsync(dto);
        return StatusCode(res.Status, res);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PromotionUpdateDto dto)
    {
        var res = await _service.UpdateAsync(id, dto);
        return StatusCode(res.Status, res);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var res = await _service.DeleteAsync(id);
        return StatusCode(res.Status, res);
    }
}