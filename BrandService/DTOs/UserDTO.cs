namespace BrandService.DTOs;

public class UserDTO
{
    public class  UserDTOLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    public class UserDTORegisterRequest
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public byte[] PasswordHash { get; set; } = null!;

        public int RoleId { get; set; }

        public Guid? DealerId { get; set; }

        public string Status { get; set; } = null!;

        public DateTime? LastActivityAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class UserDTOLoginResponse
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public Guid? DealerId { get; set; }
    }
    public class UserDTOResponse
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public byte[] PasswordHash { get; set; } = null!;

        public int RoleId { get; set; }

        public Guid? DealerId { get; set; }

        public string Status { get; set; } = null!;

        public DateTime? LastActivityAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}