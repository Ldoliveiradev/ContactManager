namespace ContactManager.Infrastructure.Identity.Exceptions;

public class UsernameAlreadyExistsException(string username)
    : Exception($"Username '{username}' is already taken.");

public class InvalidCredentialsException() : Exception("Invalid username or password.");
