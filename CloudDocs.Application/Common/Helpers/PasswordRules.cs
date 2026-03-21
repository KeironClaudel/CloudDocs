using System.Text.RegularExpressions;

namespace CloudDocs.Application.Common.Helpers;

public static class PasswordRules
{
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