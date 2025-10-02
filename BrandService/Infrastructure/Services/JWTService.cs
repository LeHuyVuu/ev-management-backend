using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrandService.DTOs.Responses.UserDTOs;
using Microsoft.IdentityModel.Tokens;

namespace BrandService.Infrastructure.Services;

public class JWTService
{
    
    private readonly IConfiguration _config;

    public JWTService(IConfiguration config)
    {
        _config = config;
    }
    public string GenerateJwtToken(UserResponse user)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new Claim("UserId", user.UserId.ToString() ?? ""),
            new Claim("RoleId", user.RoleId.ToString()??""),
            new Claim("RoleName", user.RoleName??""),
            new Claim("DealerId", user.DealerId.ToString()??""),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}