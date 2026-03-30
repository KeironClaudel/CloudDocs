using FluentValidation;

namespace CloudDocs.Application.Features.Categories.UpdateCategory;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Category name is required.")
            .MaximumLength(100)
                .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("Category description is required.")
            .MaximumLength(250)
                .WithMessage("Category description cannot exceed 250 characters.");
    }
}