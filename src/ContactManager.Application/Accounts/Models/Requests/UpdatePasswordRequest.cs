namespace ContactManager.Application.Accounts.Models.Requests;

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword);
