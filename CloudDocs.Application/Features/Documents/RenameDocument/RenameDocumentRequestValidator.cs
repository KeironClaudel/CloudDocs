using FluentValidation;

namespace CloudDocs.Application.Features.Documents.RenameDocument;

public class RenameDocumentRequestValidator : AbstractValidator<RenameDocumentRequest>
{
    public RenameDocumentRequestValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("New name is required.")
            .MaximumLength(255).WithMessage("New name cannot exceed 255 characters.");
    }
}
