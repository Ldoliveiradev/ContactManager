using ContactManager.Application.Common;

namespace ContactManager.Application.Auth.Models.Responses;

public sealed class ChangePasswordResponse : BaseResponse<bool>
{
    public static ChangePasswordResponse Success() =>
        new() { Data = true, IsSuccess = true };

    public static ChangePasswordResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
