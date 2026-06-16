using ContactManager.Infrastructure.Auth.Models;

namespace ContactManager.Infrastructure.Auth.Interfaces;

public interface IUserRepository
{
    Task<UserModel?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(UserModel user, CancellationToken ct = default);
}
