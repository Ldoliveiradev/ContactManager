namespace ContactManager.Application.Common;

public static class ErrorMessages
{
    public static class Auth
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 72;
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 100;
        public const string UsernameRequired = "Username is required.";
        public const string UsernameTooShort = "Username must be at least 3 characters.";
        public const string UsernameTooLong = "Username must be 100 characters or fewer.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordTooShort = "Password must be at least 8 characters.";
        public const string PasswordTooLong = "Password must be 72 characters or fewer.";
        public const string PasswordWeak = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.";
        public const string PasswordsDoNotMatch = "New password and confirmation do not match.";
        public const string UsernameTaken = "Username '{0}' is already taken.";
        public const string EmailTaken = "An account with this email already exists.";
        public const string InvalidCredentials = "Invalid username or password.";
        public const string UserNotFound = "User not found.";
        public const string InvalidCurrentPassword = "Current password is incorrect.";
        public const string Forbidden = "You are not allowed to access or modify this resource.";
    }

    public static class Account
    {
        public const int MinNameLength = 2;
        public const int MaxNameLength = 50;
        public const int MaxEmailLength = 200;
        public const string NotFound = "Account not found.";
        public const string FirstNameRequired = "First name is required.";
        public const string FirstNameTooShort = "First name must be at least 2 characters.";
        public const string FirstNameTooLong = "First name must be 50 characters or fewer.";
        public const string LastNameRequired = "Last name is required.";
        public const string LastNameTooShort = "Last name must be at least 2 characters.";
        public const string LastNameTooLong = "Last name must be 50 characters or fewer.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email is required.";
        public const string EmailTooLong = "Email must be 200 characters or fewer.";
        public const string InvalidCurrentPassword = "Current password is incorrect.";
    }

    public static class Contact
    {
        public const int MinNameLength = 3;
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 200;
        public const string NotFound = "Contact not found.";
        public const string NameRequired = "Name is required.";
        public const string NameTooShort = "Name must be at least 3 characters.";
        public const string NameTooLong = "Name must be 100 characters or fewer.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "A valid email is required.";
        public const string EmailTooLong = "Email must be 200 characters or fewer.";
        public const string PhoneInvalid = "Phone must be a 10-digit US number.";
        public const string EmailDuplicate = "A contact with this email already exists.";
    }
}
