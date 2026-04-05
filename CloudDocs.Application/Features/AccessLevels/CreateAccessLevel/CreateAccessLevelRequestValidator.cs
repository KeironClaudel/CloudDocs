using FluentValidation;

namespace CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;

public class CreateAccessLevelRequestValidator : AbstractValidator<CreateAccessLevelRequest>
{
    public CreateAccessLevelRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
                .WithMessage("Access level code is required.")
            .MaximumLength(50)
                .WithMessage("Access level code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Access level name is required.")
            .MaximumLength(100)
                .WithMessage("Access level name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
