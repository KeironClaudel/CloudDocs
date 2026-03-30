using FluentValidation;

namespace CloudDocs.Application.Common.Validators;

/// <summary>
/// Provides reusable FluentValidation rules for password validation.
/// </summary>
public static class PasswordValidationExtensions
{
    /// <summary>
    /// Applies the application's standard password complexity rules.
    /// Requires a minimum length of 8 characters, including uppercase,
    /// lowercase, numeric, and special characters.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");
    }
}
