using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TurnOS.Application.Interfaces;
using TurnOS.Domain.Entities;

namespace TurnOS.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "TurnOS";
        _audience = configuration["Jwt:Audience"] ?? "TurnOS";
        _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

}
