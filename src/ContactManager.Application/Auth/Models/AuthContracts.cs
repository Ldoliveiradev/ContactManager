namespace ContactManager.Application.Auth.Models;

public record RegisterRequest(string Username, string FirstName, string LastName, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record RegisterResult(Guid Id, string Username, string FullName);
public record LoginResult(string Token);
