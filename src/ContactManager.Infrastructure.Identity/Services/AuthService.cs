using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Identity.Exceptions;
using ContactManager.Infrastructure.Identity.Interfaces;
using ContactManager.Infrastructure.Identity.Models;
using ContactManager.Infrastructure.Identity.Security;
using ContactManager.Infrastructure.Identity.Validators;
using FluentValidation;

namespace ContactManager.Infrastructure.Identity.Services;

public sealed class AuthService(
    IAccountRepository accounts,
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
        if (await accounts.GetByUsernameAsync(username, ct) is not null)
            throw new UsernameAlreadyExistsException(username);

        var account = AccountDomain.Create(
            Guid.NewGuid(),
            username,
            request.FirstName,
            request.LastName,
            request.Email,
            hasher.Hash(request.Password));

        await accounts.AddAsync(account, ct);

        return new RegisterResult(account.Id, account.Username.Value, account.FullName.Value);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = LoginValidator.Validate(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var account = await accounts.GetByUsernameAsync(request.Username.Trim(), ct);

        if (account is null || !hasher.Verify(request.Password, account.PasswordHash))
            throw new InvalidCredentialsException();

        return new LoginResult(tokens.Generate(account));
    }
}
