namespace CloudDocs.API.Common;

/// <summary>
/// Provides the names of the rate limiting policies used by the API.
/// </summary>
public static class RateLimitPolicyNames
{
    /// <summary>
    /// Gets the strict policy for unauthenticated authentication endpoints.
    /// </summary>
    public const string AuthStrict = "auth-strict";
}
