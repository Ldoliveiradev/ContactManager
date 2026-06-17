using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Common;

namespace ContactManager.Application.Auth.Models.Responses;

public sealed class LoginResponse : BaseResponse<LoginResult>
{
    public static LoginResponse Success(LoginResult data) =>
        new() { Data = data, IsSuccess = true };

    public static LoginResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
