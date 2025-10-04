using FinancialService.Model;
using FinancialService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealerDiscountController : ControllerBase
{
    private readonly DealerDiscountService _service;

    public DealerDiscountController(DealerDiscountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams p)
    {
        var res = await _service.GetPagedAsync(p);
        return StatusCode(res.Status, res);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DealerDiscountCreateDto dto)
    {
        var res = await _service.CreateAsync(dto);
        return StatusCode(res.Status, res);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DealerDiscountUpdateDto dto)
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

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var res = await _service.ActivateAsync(id);
        return StatusCode(res.Status, res);
    }
}