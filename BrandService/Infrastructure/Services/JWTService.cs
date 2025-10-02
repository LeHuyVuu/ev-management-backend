using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrandService.DTOs.Responses.UserDTOs;
using Microsoft.IdentityModel.Tokens;

namespace BrandService.Infrastructure.Services
{
    public class JWTService
    {
        private readonly IConfiguration _config;

        public JWTService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(UserResponse user)
        {
            // Lấy key từ appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Các claims của user
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString() ?? string.Empty),
                new Claim("RoleId", user.RoleId.ToString() ?? string.Empty),
                new Claim("RoleName", user.RoleName ?? string.Empty),
                new Claim("DealerId", user.DealerId.ToString() ?? string.Empty)
            };

            // Tạo token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMonths(3),         
                Issuer = _config["Jwt:Issuer"],                  
                Audience = _config["Jwt:Audience"],             
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}