namespace CloudDocs.Application.Common.Interfaces.Security;

/// <summary>
/// Defines the contract for generating jwt token.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates the token.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="email">The email.</param>
    /// <param name="role">The role.</param>
    /// <returns>The string value.</returns>
    string GenerateToken(Guid userId, string email, string role);
}