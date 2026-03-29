using CloudDocs.Application.Common.Interfaces.Security;

namespace CloudDocs.Infrastructure.Security;

/// <summary>
/// Represents password hasher.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashs the password.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The string value.</returns>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies the password and hash given.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}