using FluentValidation;

namespace CloudDocs.Application.Features.Departments.CreateDepartment;

public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Department name is required.")
            .MaximumLength(100)
                .WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}