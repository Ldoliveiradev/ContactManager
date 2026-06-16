namespace ContactManager.Application.Auth;

/// <summary>
/// Application service for user registration and login (JWT issuance).
/// </summary>
public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
