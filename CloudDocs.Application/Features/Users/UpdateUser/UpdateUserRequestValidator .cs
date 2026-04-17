using FluentValidation;

namespace CloudDocs.Application.Features.Users.UpdateUser;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("Department is invalid.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role is required.");
    }
}