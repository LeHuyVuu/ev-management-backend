using BrandService.DTOs.Responses.DashboardDTOs;
using BrandService.Infrastructure.Services;
using BrandService.Model;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Infrastructure.Controller
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard overview data for Brand.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetBrandDashboard()
        {
            var result = await _dashboardService.GetDashboardAsync();
            return Ok(ApiResponse<DashboardResponse>.Success(result));
        }
    }
}
