using ContactManager.Application.Common;

namespace ContactManager.Application.Auth.Models;

public sealed class RegisterResponse : BaseResponse<RegisterResult>
{
    public static RegisterResponse Success(RegisterResult data) =>
        new() { Data = data, IsSuccess = true };

    public static RegisterResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}

public sealed class LoginResponse : BaseResponse<LoginResult>
{
    public static LoginResponse Success(LoginResult data) =>
        new() { Data = data, IsSuccess = true };

    public static LoginResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
