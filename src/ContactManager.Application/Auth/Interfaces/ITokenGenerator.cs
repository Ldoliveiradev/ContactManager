namespace ContactManager.Application.Auth.Interfaces;

public interface ITokenGenerator
{
    /// <summary>
    /// Issues a signed JWT. The subject is the shared user/account id; username
    /// (an auth attribute) and email (a business attribute) are passed in
    /// explicitly so the generator stays decoupled from both the User and the
    /// AccountDomain types.
    /// </summary>
    string Generate(Guid id, string username, string email);
}
