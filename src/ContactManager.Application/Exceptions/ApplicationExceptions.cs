namespace ContactManager.Application.Exceptions;

/// <summary>Input failed a business-rule validation. Maps to HTTP 400.</summary>
public class ValidationException(string message) : Exception(message);

/// <summary>Attempted to register a username that already exists. Maps to HTTP 409.</summary>
public class UsernameAlreadyExistsException(string username)
    : Exception($"Username '{username}' is already taken.");

/// <summary>Login failed. Deliberately generic so it does not reveal whether the
/// username exists. Maps to HTTP 401.</summary>
public class InvalidCredentialsException() : Exception("Invalid username or password.");

/// <summary>Requested resource does not exist (or is not visible to the caller).
/// Used for not-owned contacts too, so ownership is not leaked. Maps to HTTP 404.</summary>
public class NotFoundException(string message = "Resource not found.") : Exception(message);
