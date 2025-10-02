namespace IdentityService.DTOs.Responses.UserDTOs;

public class UserResponse
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = null!;
    public int RoleId { get; set; }
    public Guid? DealerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastActivityAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string RoleName  { get; set; } = null!;
    
}