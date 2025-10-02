using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.DTOs.Requests.DealerTargetDTOs;
using BrandService.DTOs.Responses.DealerTargetDTOs;
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
        private readonly DealerService _dealerService;
        private readonly DealerTargetService _dealerTargetService;

        public DealersController(DealerService dealerService, DealerTargetService dealerTargetService)
        {
            _dealerService = dealerService;
            _dealerTargetService = dealerTargetService;
        }

        // ---------------- DEALERS ----------------

        /// <summary>
        /// Get all dealers with pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var dealers = await _dealerService.GetPagedAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerResponse>>.Success(dealers));
        }

        /// <summary>
        /// Get dealer details by Id.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetById(Guid id)
        {
            var dealer = await _dealerService.GetByIdAsync(id);
            return Ok(ApiResponse<DealerResponse>.Success(dealer));
        }

        /// <summary>
        /// Create a new dealer.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] DealerRequest dealerRequest)
        {
            var created = await _dealerService.CreateAsync(dealerRequest);
            return Ok(ApiResponse<DealerResponse>.Success(created));
        }

        /// <summary>
        /// Update dealer information by Id.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DealerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(Guid id, [FromBody] DealerRequest dealerRequest)
        {
            var updated = await _dealerService.UpdateAsync(id, dealerRequest);
            return Ok(ApiResponse<DealerResponse>.Success(updated));
        }

        // ---------------- DEALER TARGETS ----------------

        /// <summary>
        /// Get all sales targets of a dealer (with pagination).
        /// </summary>
        /// <param name="dealerId">Dealer unique identifier</param>
        /// <param name="pageNumber">Page number (default = 1)</param>
        /// <param name="pageSize">Page size (default = 10)</param>
        /// <returns>Paged list of dealer targets</returns>
        [HttpGet("{dealerId}/targets")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerTargetResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetTargets(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var targets = await _dealerTargetService.GetTargetsByDealerAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerTargetResponse>>.Success(targets));
        }

        /// <summary>
        /// Get a specific target by dealer and target Id.
        /// </summary>
        [HttpGet("{dealerId}/targets/{targetId}")]
        [ProducesResponseType(typeof(ApiResponse<DealerTargetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetTargetById(Guid dealerId, Guid targetId)
        {
            var target = await _dealerTargetService.GetByIdAsync(dealerId, targetId);
            return Ok(ApiResponse<DealerTargetResponse>.Success(target));
        }

        /// <summary>
        /// Create a new sales target for a dealer.
        /// </summary>
        [HttpPost("{dealerId}/targets")]
        [ProducesResponseType(typeof(ApiResponse<DealerTargetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateTarget(Guid dealerId, [FromBody] DealerTargetRequest request)
        {
            var created = await _dealerTargetService.CreateAsync(dealerId, request);
            return Ok(ApiResponse<DealerTargetResponse>.Success(created));
        }

        /// <summary>
        /// Update a dealer's sales target.
        /// </summary>
        [HttpPut("{dealerId}/targets/{targetId}")]
        [ProducesResponseType(typeof(ApiResponse<DealerTargetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateTarget(Guid dealerId, Guid targetId, [FromBody] DealerTargetRequest request)
        {
            var updated = await _dealerTargetService.UpdateAsync(dealerId, targetId, request);
            return Ok(ApiResponse<DealerTargetResponse>.Success(updated));
        }

        /// <summary>
        /// Delete a dealer's sales target.
        /// </summary>
        [HttpDelete("{dealerId}/targets/{targetId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteTarget(Guid dealerId, Guid targetId)
        {
            await _dealerTargetService.DeleteAsync(dealerId, targetId);
            return Ok(ApiResponse<object>.Success(null, "Delete successfully."));
        }

        ///// <summary>
        ///// Update the achieved amount for a dealer's sales target.
        ///// </summary>
        //[HttpPut("{dealerId}/targets/{targetId}/achieved")]
        //[ProducesResponseType(typeof(ApiResponse<DealerTargetResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> UpdateTargetAchieved(Guid dealerId, Guid targetId)
        //{
        //    var updated = await _dealerTargetService.UpdateAchievedAmountAsync(dealerId, targetId);
        //    return Ok(ApiResponse<DealerTargetResponse>.Success(updated));
        //}
    }
}
