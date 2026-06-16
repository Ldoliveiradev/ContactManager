namespace ContactManager.Application.Common;

public static class ErrorMessages
{
    public static class Auth
    {
        public const int MinPasswordLength = 8;
        public const string UsernameRequired = "Username is required.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordTooShort = "Password must be at least 8 characters.";
        public const string UsernameTaken = "Username '{0}' is already taken.";
        public const string InvalidCredentials = "Invalid username or password.";
    }

    public static class Account
    {
        public const string NotFound = "Account not found.";
        public const string FirstNameRequired = "First name is required.";
        public const string LastNameRequired = "Last name is required.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email is required.";
        public const string InvalidCurrentPassword = "Current password is incorrect.";
    }

    public static class Contact
    {
        public const string NotFound = "Contact not found.";
        public const string NameRequired = "Name is required.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email is required.";
    }
}
