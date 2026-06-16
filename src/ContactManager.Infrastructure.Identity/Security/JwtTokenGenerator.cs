using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactManager.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace ContactManager.Infrastructure.Identity.Security;

public sealed class JwtTokenGenerator(JwtOptions options) : IJwtTokenGenerator
{
    public string Generate(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username.Value),
            new Claim(JwtRegisteredClaimNames.Email, account.Email.Value),
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
