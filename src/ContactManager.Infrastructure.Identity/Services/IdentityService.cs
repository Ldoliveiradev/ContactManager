using System.Security.Claims;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Identity.Interfaces;
using ContactManager.Infrastructure.Identity.Security;

namespace ContactManager.Infrastructure.Identity.Services;

public sealed class IdentityService(
    IPasswordHasher hasher,
    IJwtTokenGenerator tokens) : IIdentityService
{
    public string HashPassword(string password) => hasher.Hash(password);
    public bool VerifyPassword(string password, string passwordHash) => hasher.Verify(password, passwordHash);
    public string GenerateToken(AccountDomain account) => tokens.Generate(account);

    public bool IsCurrentUser(Guid accountId, ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? principal.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) && id == accountId;
    }
}
