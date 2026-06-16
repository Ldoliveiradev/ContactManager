using ContactManager.Domain.Entities;

namespace ContactManager.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
