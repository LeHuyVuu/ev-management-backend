using Microsoft.AspNetCore.Mvc;
using BrandService.DTOs;
using BrandService.Infrastructure.Services;
using BrandService.Models;

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
        [ProducesResponseType(typeof(ApiResponse<List<DealerDTO.DealerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var dealers = await _service.GetAllAsync();
                return Ok(ApiResponse<List<DealerDTO.DealerResponse>>.Success(dealers));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(500, "An error occurred while retrieving dealers", ex.Message));
            }
        }

        /// <summary>
        /// Get dealer details by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <returns>Dealer details if found, otherwise 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerDTO.DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(Guid id)
        {
            try
            {
                var dealer = await _service.GetByIdAsync(id);
                if (dealer == null)
                    return NotFound(ApiResponse<object>.Fail(404, "Dealer not found"));
                return Ok(ApiResponse<DealerDTO.DealerResponse>.Success(dealer));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(500, "An error occurred while retrieving the dealer", ex.Message));
            }
        }

        /// <summary>
        /// Create a new dealer.
        /// </summary>
        /// <param name="request">Dealer information to be created.</param>
        /// <returns>The created dealer.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DealerDTO.DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] DealerDTO.DealerRequest dealerRequest)
        {
            try
            {
                var created = await _service.CreateAsync(dealerRequest);
                if (created == null)
                    return BadRequest(ApiResponse<object>.Fail(400, "Failed to create dealer"));
                return Ok(ApiResponse<DealerDTO.DealerResponse>.Success(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(500, "An error occurred while adding the dealer", ex.Message));
            }
        }

        /// <summary>
        /// Update dealer information by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <param name="request">Dealer information to update.</param>
        /// <returns>The updated dealer.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerDTO.DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] DealerDTO.DealerRequest dealerRequest)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dealerRequest);
                if (updated == null)
                    return BadRequest(ApiResponse<object>.Fail(404, "Dealer not found"));
                return Ok(ApiResponse<DealerDTO.DealerResponse>.Success(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(400, "Failed to update dealer", ex.Message));
            }
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
