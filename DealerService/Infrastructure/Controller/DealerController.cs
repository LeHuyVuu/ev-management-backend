using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealersController : ControllerBase
    {
        private readonly DealerService _service;

        public DealersController(DealerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<DealerDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DealerDto>> GetById(Guid id)
        {
            var dealer = await _service.GetByIdAsync(id);
            if (dealer == null) return NotFound();
            return Ok(dealer);
        }

        [HttpPost]
        public async Task<ActionResult<DealerDto.DealerResponse>> Create([FromBody] DealerDto.DealerRequest dealerRequest)
        {
            var created = await _service.CreateAsync(dealerRequest);
            return CreatedAtAction(nameof(GetById), new { id = created.DealerId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DealerDto.DealerResponse>> Update(Guid id, [FromBody] DealerDto.DealerRequest dealerRequest)
        {
            var updated = await _service.UpdateAsync(id, dealerRequest);
            return Ok(updated);
        }

        [HttpGet("{id}/targets")]
        public async Task<ActionResult<List<DealerTargetDto>>> GetTargets(Guid id)
        {
            return Ok(await _service.GetTargetsAsync(id));
        }
    }

}
