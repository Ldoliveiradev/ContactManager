using ContactManager.Infrastructure.Identity.Models;

namespace ContactManager.Infrastructure.Identity.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
