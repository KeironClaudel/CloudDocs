using FluentValidation;

namespace CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;

public class UpdateAccessLevelRequestValidator : AbstractValidator<UpdateAccessLevelRequest>
{
    public UpdateAccessLevelRequestValidator()
    {
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