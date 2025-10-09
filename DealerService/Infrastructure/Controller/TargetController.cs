using DealerService.DTOs.Responses.DealerTargetDTOs;
using DealerService.Infrastructure.Services;
using DealerService.Models;
using Microsoft.AspNetCore.Mvc;

namespace DealerService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealerTargetsController : ControllerBase
    {
        private readonly DealerTargetService _dealerTargetService;

        public DealerTargetsController(DealerTargetService dealerTargetService)
        {
            _dealerTargetService = dealerTargetService;
        }

        /// <summary>
        /// Get all dealer targets with pagination.
        /// </summary>
        /// <param name="pageNumber">Page number (default = 1)</param>
        /// <param name="pageSize">Page size (default = 10)</param>
        /// <returns>Paged list of dealer targets.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerTargetResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var targets = await _dealerTargetService.GetPagedAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerTargetResponse>>.Success(targets));
        }
    }
}
