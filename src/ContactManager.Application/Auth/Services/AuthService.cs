using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Validators;
using ContactManager.Application.Common;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Auth.Services;

public sealed class AuthService(
    IAccountRepository accounts,
    IPasswordHasher hasher,
    ITokenGenerator tokens) : IAuthService
{
    private static readonly RegisterRequestValidator RegisterValidator = new();
    private static readonly LoginRequestValidator LoginValidator = new();

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validation = RegisterValidator.Validate(request);
        if (!validation.IsValid)
            return RegisterResponse.Failure(validation.Errors[0].ErrorMessage);

        var username = request.Username.Trim();
        if (await accounts.GetByUsernameAsync(username, ct) is not null)
            return RegisterResponse.Failure(string.Format(ErrorMessages.Auth.UsernameTaken, username));

        var account = AccountDomain.Create(
            Guid.NewGuid(),
            username,
            request.FirstName,
            request.LastName,
            request.Email,
            hasher.Hash(request.Password));

        await accounts.AddAsync(account, ct);

        return RegisterResponse.Success(new RegisterResult(account.Id, account.Username.Value, account.FullName.Value));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = LoginValidator.Validate(request);
        if (!validation.IsValid)
            return LoginResponse.Failure(validation.Errors[0].ErrorMessage);

        var account = await accounts.GetByUsernameAsync(request.Username.Trim(), ct);

        if (account is null || !hasher.Verify(request.Password, account.PasswordHash))
            return LoginResponse.Failure(ErrorMessages.Auth.InvalidCredentials);

        return LoginResponse.Success(new LoginResult(tokens.Generate(account)));
    }
}
