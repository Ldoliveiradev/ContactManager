namespace ContactManager.Application.Auth.Models.Requests;

public record RegisterRequest(string Username, string FirstName, string LastName, string Email, string Password);
