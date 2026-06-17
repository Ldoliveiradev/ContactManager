using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Common;

namespace ContactManager.Application.Auth.Models.Responses;

public sealed class RegisterResponse : BaseResponse<RegisterResult>
{
    public static RegisterResponse Success(RegisterResult data) =>
        new() { Data = data, IsSuccess = true };

    public static RegisterResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
