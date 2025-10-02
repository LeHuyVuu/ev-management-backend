namespace IdentityService.DTOs.Requests.UserDTOs;

public class UserLoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}