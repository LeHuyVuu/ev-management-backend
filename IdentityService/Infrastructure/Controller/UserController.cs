using System.Security.Claims;
using IdentityService.DTOs.Requests.UserDTOs;
using IdentityService.DTOs.Responses.UserDTOs;
using IdentityService.Infrastructure.Services;
using IdentityService.Model;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserService _service;

    public UserController(ILogger<UserController> logger, UserService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// GET /api/users/me
    /// Lấy thông tin user hiện tại (dùng UserId từ token)
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<UserResponse>.Fail(400, "Không tìm thấy UserId trong token"));

        var user = await _service.GetUserById(userId);
        if (user == null)
            return NotFound(ApiResponse<UserResponse>.Fail(404, "Không tìm thấy user"));

        return Ok(ApiResponse<UserResponse>.Success(user));
    }
    

    /// <summary>
    /// PUT /api/users/{id}
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(Guid id, [FromBody] UserUpdateRequest request)
    {
        if (id != request.UserId)
            return BadRequest(ApiResponse<UserResponse>.Fail(400, "Id không khớp với dữ liệu gửi lên"));

        var updated = await _service.UpdateUser(request);
        if (updated == null)
            return NotFound(ApiResponse<UserResponse>.Fail(404, "Không tìm thấy user"));

        return Ok(ApiResponse<UserResponse>.Success(updated, "Cập nhật user thành công"));
    }

    /// <summary>
    /// PATCH /api/users/{id}/status
    /// Kích hoạt / khóa user
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id, [FromBody] string status)
    {
        var result = await _service.UpdateStatusUser(id, status);
        if (!result)
            return NotFound(ApiResponse<object>.Fail(404, "Không tìm thấy user hoặc cập nhật thất bại"));

        return Ok(ApiResponse<object>.Success(null, $"Cập nhật trạng thái user thành {status}"));
    }
}
