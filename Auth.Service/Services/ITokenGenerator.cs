using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Service.Services;

public interface ITokenGenerator
{
    string Generate();
}

public class TokenGenerator : ITokenGenerator
{
    public string Generate()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey("ДАДАДАДАНЕТНЕТНЕТНЕТ"u8.ToArray());
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials, expires: DateTime.Now.AddMinutes(60));
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}