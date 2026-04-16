using FluentValidation;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
                .WithMessage("Original file name is required.")
            .MaximumLength(255)
                .WithMessage("Original file name cannot exceed 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
                .WithMessage("Content type is required.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
                .WithMessage("File is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
                .WithMessage("Category is required.");

        RuleFor(x => x.DocumentTypeId)
            .NotEmpty()
                .WithMessage("Document type is required.");

        RuleFor(x => x.AccessLevelId)
            .NotEmpty()
                .WithMessage("Access level is required.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
                .WithMessage("Client is required.");

        RuleFor(x => x.ExpirationDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow.Date)
            .WithMessage("Expiration date must be greater than today.")
            .When(x => x.ExpirationDate.HasValue);
    }
}