namespace CloudDocs.Application.Common.Interfaces.Security;

/// <summary>
/// Defines the contract for generating refresh token.
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Generates.
    /// </summary>
    /// <returns>The string value.</returns>
    string Generate();
}