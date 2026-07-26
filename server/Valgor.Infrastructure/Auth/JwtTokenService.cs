using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Valgor.Application.Common.Interfaces;

namespace Valgor.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock) : IJwtTokenService
{
    public string CreateToken(Guid userId, string email, string role, string displayName)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SigningKey) || settings.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured with at least 32 characters.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = clock.UtcNow.AddMinutes(settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: clock.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
