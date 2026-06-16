namespace ContactManager.Infrastructure.Services;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public record RegisterResult(Guid Id, string Username);

public record LoginResult(string Token);
