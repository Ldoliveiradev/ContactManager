namespace ContactManager.Domain.Exceptions;

public class NotFoundException(string message = "Resource not found.") : Exception(message);

public class UsernameAlreadyExistsException(string username)
    : Exception($"Username '{username}' is already taken.");

public class InvalidCredentialsException() : Exception("Invalid username or password.");
