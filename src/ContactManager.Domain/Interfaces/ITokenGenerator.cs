using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface ITokenGenerator
{
    string Generate(AccountDomain account);
}
