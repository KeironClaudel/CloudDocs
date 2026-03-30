using CloudDocs.Application.Common.Validators;
using FluentValidation;

namespace CloudDocs.Application.Features.Auth.ChangePassword;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
                .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithMessage("New password is required.")
            .ApplyPasswordRules();
    }
}