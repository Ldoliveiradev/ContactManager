using ContactManager.Domain.Entities;

namespace ContactManager.Application.Abstractions;

/// <summary>
/// Issues a signed JWT for an authenticated user. Implemented in Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    string Generate(User user);
}
