using ContactManager.Domain.Entities;
using ContactManager.Domain.Exceptions;
using ContactManager.Domain.Repositories;
using ContactManager.Infrastructure.Security;
using FluentValidation;

namespace ContactManager.Infrastructure.Services;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenGenerator tokens) : IAuthService
{
    private static readonly RegisterRequestValidator RegisterValidator = new();
    private static readonly LoginRequestValidator LoginValidator = new();

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validation = RegisterValidator.Validate(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var username = request.Username.Trim();
        if (await users.GetByUsernameAsync(username, ct) is not null)
            throw new UsernameAlreadyExistsException(username);

        var user = User.Create(Guid.NewGuid(), username, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);

        return new RegisterResult(user.Id, user.Username);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = LoginValidator.Validate(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var user = await users.GetByUsernameAsync(request.Username.Trim(), ct);

        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return new LoginResult(tokens.Generate(user));
    }
}
