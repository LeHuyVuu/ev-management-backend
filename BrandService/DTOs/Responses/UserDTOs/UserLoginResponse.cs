namespace BrandService.DTOs.Responses.UserDTOs;

public class UserLoginResponse
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
    public Guid? DealerId { get; set; }
}