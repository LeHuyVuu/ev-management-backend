namespace BrandService.DTOs.Requests.UserDTOs;

public class UserRegisterRequest
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public byte[] PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public Guid? DealerId { get; set; }

    public string Status { get; set; } = null!;
}