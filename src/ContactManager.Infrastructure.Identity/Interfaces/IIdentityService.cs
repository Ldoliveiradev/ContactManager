using ContactManager.Domain.Models;

namespace ContactManager.Infrastructure.Identity.Interfaces;

public interface IIdentityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateToken(AccountDomain account);
    bool IsCurrentUser(Guid accountId, System.Security.Claims.ClaimsPrincipal principal);
}
