namespace ContactManager.Application.Auth.Models.Requests;

public record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword, string ConfirmNewPassword);
