using IdentityService.DTOs.Requests.UserDTOs;
using IdentityService.DTOs.Responses.RoleDTOs;
using IdentityService.DTOs.Responses.UserDTOs;
using IdentityService.Infrastructure.Services;
using IdentityService.Model;
using IdentityService.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Infrastructure.Controller;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserService _service;
    private readonly RoleService _roleService;

    public AdminController(ILogger<AdminController> logger, UserService service, RoleService roleService)
    {
        _logger = logger;
        _service = service;
        _roleService = roleService;
    }

    /// <summary>
    /// GET /api/admin/users
    /// Lấy danh sách user (phân trang)
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetUsers([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var users = await _service.GetUsersAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<UserResponse>>.Success(users, "Lấy danh sách user thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách user");
            return StatusCode(500, ApiResponse<PagedResult<UserResponse>>.Fail(500, "Có lỗi xảy ra khi lấy danh sách user"));
        }
    }

    /// <summary>
    /// GET /api/admin/roles
    /// Lấy danh sách role
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponse>>>> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRoles();
            return Ok(ApiResponse<IEnumerable<RoleResponse>>.Success(roles, "Lấy danh sách role thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách role");
            return StatusCode(500, ApiResponse<IEnumerable<RoleResponse>>.Fail(500, "Có lỗi xảy ra khi lấy danh sách role"));
        }
    }

    /// <summary>
    /// POST /api/admin/users
    /// Tạo user mới
    /// </summary>
    [HttpPost("users")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] UserRegisterRequest request)
    {
        try
        {
            // Hash password trước khi lưu
            request.PasswordHash = PasswordHelper.HashPassword(request.PasswordHash);

            var created = await _service.CreateUser(request);
            if (created == null)
            {
                return BadRequest(ApiResponse<UserResponse>.Fail(400, "Không thể tạo user"));
            }

            return Ok(ApiResponse<UserResponse>.Success(created, "Tạo mới user thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo user");
            return StatusCode(500, ApiResponse<UserResponse>.Fail(500, "Có lỗi xảy ra khi tạo user"));
        }
    }

    /// <summary>
    /// PATCH /api/admin/users/{id}/role
    /// Cập nhật role user
    /// </summary>
    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRoleUser(Guid id, [FromQuery] int roleId)
    {
        try
        {
            var result = await _service.UpdateRoleUser(id, roleId);
            if (!result)
            {
                _logger.LogWarning("Không thể cập nhật role cho user {UserId}", id);
                return NotFound(ApiResponse<bool>.Fail(404, "Không tìm thấy user hoặc không thể cập nhật role"));
            }

            return Ok(ApiResponse<bool>.Success(true, "Cập nhật role user thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật role cho user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.Fail(500, "Có lỗi xảy ra khi cập nhật role user"));
        }
    }

    /// <summary>
    /// PUT /api/admin/users/{id}
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("users/{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(Guid id, [FromBody] UserUpdateRequest request)
    {
        try
        {
            if (id != request.UserId)
            {
                return BadRequest(ApiResponse<UserResponse>.Fail(400, "Id trong route không khớp với body"));
            }

            var updated = await _service.UpdateUser(request);
            if (updated == null)
            {
                _logger.LogWarning("Không tìm thấy user {UserId} để cập nhật", id);
                return NotFound(ApiResponse<UserResponse>.Fail(404, "Không tìm thấy user hoặc không thể cập nhật"));
            }

            return Ok(ApiResponse<UserResponse>.Success(updated, "Cập nhật user thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật user {UserId}", id);
            return StatusCode(500, ApiResponse<UserResponse>.Fail(500, "Có lỗi xảy ra khi cập nhật user"));
        }
    }

    /// <summary>
    /// PATCH /api/admin/users/{id}/status
    /// Kích hoạt / khóa user
    /// </summary>
    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(Guid id, [FromQuery] string status)
    {
        try
        {
            var updated = await _service.UpdateStatusUser(id, status);
            if (!updated)
            {
                _logger.LogWarning("Không tìm thấy user {UserId} để cập nhật trạng thái", id);
                return NotFound(ApiResponse<bool>.Fail(404, "Không tìm thấy user hoặc không thể cập nhật trạng thái"));
            }

            return Ok(ApiResponse<bool>.Success(true, "Cập nhật trạng thái user thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái user {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.Fail(500, "Có lỗi xảy ra khi cập nhật trạng thái user"));
        }
    }
}
