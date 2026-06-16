namespace ContactManager.Application.Auth;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

/// <summary>Result of a successful registration.</summary>
public record RegisterResult(Guid Id, string Username);

/// <summary>Result of a successful login — the signed JWT.</summary>
public record LoginResult(string Token);
