using ContactManager.Domain.Entities;

namespace ContactManager.Application.Abstractions;

/// <summary>
/// Persistence contract for users. Implemented in Infrastructure with hand-written SQL.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
