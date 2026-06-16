namespace ContactManager.Application.Abstractions;

/// <summary>
/// Hashes and verifies passwords. Implemented in Infrastructure (PBKDF2) so the
/// Application layer stays free of cryptographic / framework concerns.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
