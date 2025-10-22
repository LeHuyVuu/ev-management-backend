using DealerService.DTOs.Responses.DealerDTOs;
using DealerService.DTOs.Responses.DealerTargetDTOs;
using DealerService.Infrastructure.Services;
using DealerService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DealerService.Infrastructure.Controller;

[ApiController]
[Route("api")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService) 
    { 
        _userService = userService;
    }

    /// <summary>
    /// Lấy ra các user có role là Dealer Staff
    /// </summary>
    /// <returns></returns>
    [HttpGet("/users/dealer-staffs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerTargetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DealerTargetResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserByRoleDealerStaff()
    {
        var dealerStaff = await _userService.GetUserByRoleDealerStaff();
        return Ok(ApiResponse<IEnumerable<DealerStaffsResponse>>.Success(dealerStaff));
    }
}