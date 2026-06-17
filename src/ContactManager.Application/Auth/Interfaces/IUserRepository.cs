using ContactManager.Application.Auth.Models;

namespace ContactManager.Application.Auth.Interfaces;

/// <summary>
/// Persistence for the authentication identity. Implemented in the Identity
/// infrastructure layer; auth data is kept separate from the business
/// repositories.
/// </summary>
public interface IUserRepository
{
    Task<UserModel?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<UserModel?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<UserModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(UserModel user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(UserModel user, CancellationToken ct = default);
}
