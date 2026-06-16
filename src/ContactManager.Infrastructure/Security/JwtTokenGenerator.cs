using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactManager.Application.Abstractions;
using ContactManager.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ContactManager.Infrastructure.Security;

/// <summary>
/// Issues HMAC-SHA256-signed JWTs carrying the user's id (as the subject) and username.
/// The owning user id is later read from these claims by the API — never from request bodies.
/// </summary>
public sealed class JwtTokenGenerator(JwtOptions options) : IJwtTokenGenerator
{
    public string Generate(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
