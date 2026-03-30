using FluentValidation;

namespace CloudDocs.Application.Features.Categories.CreateCategory;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Category name is required.")
            .MaximumLength(100)
                .WithMessage("Category name cannot exceed 100 characters.");
    }
}