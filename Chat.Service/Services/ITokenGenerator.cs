using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chat.Service.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Service.Services;

public interface ITokenGenerator
{
    string Generate(string name);
}

public class TokenGenerator(IOptionsMonitor<JwtOptions> monitor) : ITokenGenerator
{
    private readonly JwtOptions _jwtOptions = monitor.CurrentValue;
    
    public string Generate(string name)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials, expires: DateTime.Now.AddMinutes(60));
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}