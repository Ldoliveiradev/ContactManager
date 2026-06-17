using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactManager.Application.Auth.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ContactManager.Infrastructure.Identity.Security;

public sealed class JwtTokenGenerator(JwtOptions options, TimeProvider timeProvider) : ITokenGenerator
{
    public string Generate(Guid id, string username, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Use the injected clock so token expiry is deterministic and testable.
        var expires = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
