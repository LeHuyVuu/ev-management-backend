namespace IdentityService.DTOs.Responses.RoleDTOs;

public class RoleResponse
{
    public int RoleId { get; set; }

    public string RoleKey { get; set; } = null!;

    public string RoleName { get; set; } = null!;
}