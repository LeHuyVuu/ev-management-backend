using Application.ExceptionHandler;
using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using BrandService.Models;
using Microsoft.AspNetCore.Mvc;

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
        /// Get all dealers with pagination.
        /// </summary>
        /// <param name="pageNumber">The page number (default = 1).</param>
        /// <param name="pageSize">The number of records per page (default = 10).</param>
        /// <returns>Paged list of dealers.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DealerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var dealers = await _service.GetPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerResponse>>.Success(dealers));
        }

        /// <summary>
        /// Get dealer details by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <returns>Dealer details if found, otherwise 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(Guid id)
        {
            var dealer = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<DealerResponse>.Success(dealer));
        }

        /// <summary>
        /// Create a new dealer.
        /// </summary>
        /// <param name="request">Dealer information to be created.</param>
        /// <returns>The created dealer.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] DealerRequest dealerRequest)
        {
            var created = await _service.CreateAsync(dealerRequest);
            return Ok(ApiResponse<DealerResponse>.Success(created));
        }

        /// <summary>
        /// Update dealer information by Id.
        /// </summary>
        /// <param name="id">Dealer unique identifier (GUID).</param>
        /// <param name="request">Dealer information to update.</param>
        /// <returns>The updated dealer.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] DealerRequest dealerRequest)
        {
            var updated = await _service.UpdateAsync(id, dealerRequest);
            return Ok(ApiResponse<DealerResponse>.Success(updated));
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
