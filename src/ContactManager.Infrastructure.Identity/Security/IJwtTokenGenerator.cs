using ContactManager.Domain.Models;

namespace ContactManager.Infrastructure.Identity.Security;

public interface IJwtTokenGenerator
{
    string Generate(AccountDomain account);
}
