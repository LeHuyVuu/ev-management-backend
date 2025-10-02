using IdentityService.DTOs.Requests.UserDTOs;
using IdentityService.DTOs.Responses.UserDTOs;
using IdentityService.Infrastructure.Services;
using IdentityService.Model;
using IdentityService.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Infrastructure.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly UserService _userService;
    private readonly JWTService _jwtService;

    public AuthController(ILogger<AuthController> logger, UserService userService, JWTService jwtService)
    {
        _logger = logger;
        _userService = userService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// POST /api/auth/login
    /// Đăng nhập bằng email + password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserLoginResponse>>> Login([FromBody] UserLoginRequest request)
    {
        var user = await _userService.GetUserByEmail(request.Email);
        if (user == null)
        {
            return Unauthorized(ApiResponse<UserLoginResponse>.Fail(401, "Email không tồn tại"));
        }

        // Verify password
        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<UserLoginResponse>.Fail(401, "Sai mật khẩu"));
        }

        // Generate token
        var token = _jwtService.GenerateJwtToken(user);
        var response = new UserLoginResponse
        {
            Token = token,
        };

        return Ok(ApiResponse<UserLoginResponse>.Success(response, "Đăng nhập thành công"));
    }
    
    /// <summary>
    /// POST /api/users/register
    /// Đăng ký user mới
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] UserRegisterRequest request)
    {
        // Hash password trước khi lưu
        request.PasswordHash = PasswordHelper.HashPassword(request.PasswordHash);

        var created = await _userService.CreateUser(request);
        if (created == null)
        {
            return BadRequest(ApiResponse<UserResponse>.Fail(400, "Không thể tạo user"));
        }

        return Ok(ApiResponse<UserResponse>.Success(created, "Đăng ký user thành công"));
    }
}