using System.Security.Cryptography;
using CloudDocs.Application.Common.Interfaces.Security;

namespace CloudDocs.Infrastructure.Security;

/// <summary>
/// Represents refresh token generator.
/// </summary>
public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a random 64 bites string.
    /// </summary>
    /// <returns>The string value.</returns>
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}