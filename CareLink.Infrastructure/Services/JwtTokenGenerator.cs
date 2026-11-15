using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareLink.Core.Entities;
using CareLink.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CareLink.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "CareLinkAPI";
        var audience = _configuration["Jwt:Audience"] ?? "CareLinkClient";
        var expiryHours = int.TryParse(_configuration["Jwt:ExpiryHours"], out var hours) ? hours : 24;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organizationId", user.OrganizationId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
