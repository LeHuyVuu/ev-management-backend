using Microsoft.AspNetCore.Mvc;
using BrandService.DTOs;
using BrandService.Infrastructure.Services;

namespace BrandService.Infrastructure.Controller
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

        /// <summary>
        /// Get all dealers.
        /// </summary>
        /// <returns>A list of dealers.</returns>
        [HttpGet]
        public async Task<ActionResult<List<DealerDto.DealerResponse>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>
        /// Get dealer details by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <returns>Dealer details if found, otherwise 404.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DealerDto.DealerResponse>> GetById(Guid id)
        {
            var dealer = await _service.GetByIdAsync(id);
            if (dealer == null) return NotFound();
            return Ok(dealer);
        }

        /// <summary>
        /// Create a new dealer.
        /// </summary>
        /// <param name="request">Dealer information to be created.</param>
        /// <returns>The created dealer.</returns>
        [HttpPost]
        public async Task<ActionResult<DealerDto.DealerResponse>> Create([FromBody] DealerDto.DealerRequest dealerRequest)
        {
            var created = await _service.CreateAsync(dealerRequest);
            return CreatedAtAction(nameof(GetById), new { id = created.DealerId }, created);
        }

        /// <summary>
        /// Update dealer information by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <param name="request">Dealer information to update.</param>
        /// <returns>The updated dealer.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<DealerDto.DealerResponse>> Update(Guid id, [FromBody] DealerDto.DealerRequest dealerRequest)
        {
            var updated = await _service.UpdateAsync(id, dealerRequest);
            return Ok(updated);
        }

        /// <summary>
        /// Get sales targets assigned to a dealer.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <returns>A list of sales targets for the dealer.</returns>
        //[HttpGet("{id}/targets")]
        //public async Task<ActionResult<List<DealerTargetDto>>> GetTargets(Guid id)
        //{
        //    return Ok(await _service.GetTargetsAsync(id));
        //}
    }

}
