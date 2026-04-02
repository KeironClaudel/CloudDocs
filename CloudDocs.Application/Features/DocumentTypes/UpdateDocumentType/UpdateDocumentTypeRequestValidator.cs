using FluentValidation;

namespace CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;

public class UpdateDocumentTypeRequestValidator : AbstractValidator<UpdateDocumentTypeRequest>
{
    public UpdateDocumentTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Document type name is required.")
            .MaximumLength(100)
                .WithMessage("Document type name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}