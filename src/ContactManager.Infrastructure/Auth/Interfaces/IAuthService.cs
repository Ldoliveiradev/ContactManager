using ContactManager.Infrastructure.Auth.Models;

namespace ContactManager.Infrastructure.Auth.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
