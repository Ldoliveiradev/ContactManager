using ContactManager.Application.Abstractions;
using ContactManager.Application.Exceptions;
using ContactManager.Domain.Entities;

namespace ContactManager.Application.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenGenerator tokens) : IAuthService
{
    private const int MinPasswordLength = 8;

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        Validate(request.Username, request.Password);

        var username = request.Username.Trim();
        if (await users.GetByUsernameAsync(username, ct) is not null)
            throw new UsernameAlreadyExistsException(username);

        var user = User.Create(Guid.NewGuid(), username, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);

        return new RegisterResult(user.Id, user.Username);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByUsernameAsync(request.Username.Trim(), ct);

        // Verify even on a miss would be ideal to avoid timing leaks, but at minimum
        // we return the same exception for "no user" and "wrong password".
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return new LoginResult(tokens.Generate(user));
    }

    private static void Validate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username is required.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException("Password is required.");
        if (password.Length < MinPasswordLength)
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters.");
    }
}
