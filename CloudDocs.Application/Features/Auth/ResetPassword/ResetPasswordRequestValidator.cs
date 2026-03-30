using CloudDocs.Application.Common.Validators;
using FluentValidation;

namespace CloudDocs.Application.Features.Auth.ResetPassword;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
                .WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithMessage("New password is required.")
            .ApplyPasswordRules();
    }
}