using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using ContactManager.Application.Auth.Validators;
using ContactManager.Application.Common;
using ContactManager.Domain.Exceptions;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Auth.Services;

public sealed class AuthService(
    IUserRepository users,
    IAccountRepository accounts,
    IPasswordHasher hasher,
    ITokenGenerator tokens) : IAuthService
{
    private static readonly RegisterRequestValidator RegisterValidator = new();
    private static readonly LoginRequestValidator LoginValidator = new();
    private static readonly ChangePasswordRequestValidator ChangePasswordValidator = new();

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validation = RegisterValidator.Validate(request);
        if (!validation.IsValid)
            return RegisterResponse.Failure(validation.Errors[0].ErrorMessage);

        var username = request.Username.Trim();
        if (await users.GetByUsernameAsync(username, ct) is not null)
            return RegisterResponse.Failure(string.Format(ErrorMessages.Auth.UsernameTaken, username));

        var email = request.Email.Trim();
        if (await accounts.ExistsByEmailAsync(email, ct))
            return RegisterResponse.Failure(ErrorMessages.Auth.EmailTaken);

        // A user (auth) and their account (business) are created together and share
        // the same id, so the JWT subject identifies both.
        var id = Guid.NewGuid();
        var user = UserModel.Create(id, username, hasher.Hash(request.Password));
        var account = AccountDomain.Create(id, request.FirstName, request.LastName, request.Email);

        try
        {
            await users.AddAsync(user, ct);
        }
        catch (DuplicateUsernameException)
        {
            return RegisterResponse.Failure(string.Format(ErrorMessages.Auth.UsernameTaken, username));
        }

        try
        {
            await accounts.AddAsync(account, ct);
        }
        catch (DuplicateAccountEmailException)
        {
            await users.DeleteAsync(id, ct);
            return RegisterResponse.Failure(ErrorMessages.Auth.EmailTaken);
        }

        return RegisterResponse.Success(new RegisterResult(id, user.Username, account.FullName.Value));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = LoginValidator.Validate(request);
        if (!validation.IsValid)
            return LoginResponse.Failure(validation.Errors[0].ErrorMessage);

        var identifier = request.Username.Trim();
        var user = await users.GetByUsernameAsync(identifier, ct)
                   ?? await users.GetByEmailAsync(identifier, ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            return LoginResponse.Failure(ErrorMessages.Auth.InvalidCredentials);

        // Email lives on the business account (shared id); load it for the token claim.
        var account = await accounts.GetByIdAsync(user.Id, ct);
        var email = account?.Email.Value ?? string.Empty;

        return LoginResponse.Success(new LoginResult(tokens.Generate(user.Id, user.Username, email)));
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var validation = ChangePasswordValidator.Validate(request);
        if (!validation.IsValid)
            return ChangePasswordResponse.Failure(validation.Errors[0].ErrorMessage);

        var user = await users.GetByIdAsync(userId, ct);
        if (user is null)
            return ChangePasswordResponse.Failure(ErrorMessages.Auth.UserNotFound);

        if (!hasher.Verify(request.CurrentPassword, user.PasswordHash))
            return ChangePasswordResponse.Failure(ErrorMessages.Auth.InvalidCurrentPassword);

        user.ChangePassword(hasher.Hash(request.NewPassword));
        await users.UpdateAsync(user, ct);
        return ChangePasswordResponse.Success();
    }
}
