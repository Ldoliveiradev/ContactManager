using ContactManager.Domain.Models;

namespace ContactManager.Infrastructure.Identity.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(AccountDomain account);
}
