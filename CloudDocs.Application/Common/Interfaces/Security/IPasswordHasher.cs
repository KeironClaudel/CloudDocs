namespace CloudDocs.Application.Common.Interfaces.Security;

/// <summary>
/// Defines the contract for hashing and verifying password.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashs.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The string value.</returns>
    string Hash(string password);
    /// <summary>
    /// Verifies.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    bool Verify(string password, string passwordHash);
}