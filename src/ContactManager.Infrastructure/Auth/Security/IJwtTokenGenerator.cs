using ContactManager.Infrastructure.Auth.Models;

namespace ContactManager.Infrastructure.Auth.Security;

public interface IJwtTokenGenerator
{
    string Generate(UserModel user);
}
