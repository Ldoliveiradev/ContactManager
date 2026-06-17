namespace ContactManager.Domain.Exceptions;

public class NotFoundException(string message = "Resource not found.") : Exception(message);

public class ContactNotFoundException()
    : NotFoundException("Contact not found.");

public class AccountNotFoundException()
    : NotFoundException("Account not found.");

public class DuplicateContactEmailException(string email)
    : Exception($"A contact with email '{email}' already exists in this account.");

public class DuplicateUsernameException(string username)
    : Exception($"Username '{username}' is already taken.");

public class DuplicateAccountEmailException(string email)
    : Exception($"An account with email '{email}' already exists.");
