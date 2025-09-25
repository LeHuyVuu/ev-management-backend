using Microsoft.AspNetCore.Mvc;
using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Models;
using Application.ExceptionHandler;

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
        [ProducesResponseType(typeof(ApiResponse<List<DealerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var dealers = await _service.GetAllAsync();
                return Ok(ApiResponse<List<DealerResponse>>.Success(dealers));
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving dealers");
            }
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
            try
            {
                var dealer = await _service.GetByIdAsync(id);
                if (dealer == null)
                    throw new NotFoundException("Dealer not found");
                return Ok(ApiResponse<DealerResponse>.Success(dealer));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
            try
            {
                var created = await _service.CreateAsync(dealerRequest);
                if (created == null)
                    throw new BadRequestException("Failed to create dealer");

                return Ok(ApiResponse<DealerResponse>.Success(created));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create dealer");
            }
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
            try
            {
                var updated = await _service.UpdateAsync(id, dealerRequest);
                if (updated == null)
                    throw new NotFoundException("Dealer not found");
                return Ok(ApiResponse<DealerResponse>.Success(updated));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update dealer");
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
