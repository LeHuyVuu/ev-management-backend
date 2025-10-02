namespace IdentityService.DTOs.Requests.UserDTOs;

public class UserUpdateRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public byte[] PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public Guid? DealerId { get; set; }

    public string Status { get; set; } = null!;
}