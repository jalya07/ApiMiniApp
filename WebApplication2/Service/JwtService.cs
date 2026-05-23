using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Entities;

namespace WebApplication2.Service;

public class JwtService
{
    public string GenerateToken(AppUser user,IList<string> roles,IConfiguration config)
    {
        var claims = new List<Claim>
        { 
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("FullName", user.FullName)
        }; 
        claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken=new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds);
        
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return token;
        
    }
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}