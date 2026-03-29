using System.Text.RegularExpressions;

namespace CloudDocs.Application.Common.Helpers;

/// <summary>
/// Provides shared password validation rules.
/// </summary>
public static class PasswordRules
{
    /// <summary>
    /// Validates whether a password satisfies the application's security policy.
    /// Requires a minimum length of 8 characters, including uppercase, lowercase,
    /// numeric, and special characters.
    /// </summary>
    public static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < 8)
            return false;

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        if (!Regex.IsMatch(password, @"[a-z]"))
            return false;

        if (!Regex.IsMatch(password, @"[0-9]"))
            return false;

        if (!Regex.IsMatch(password, @"[\W_]"))
            return false;

        return true;
    }
}