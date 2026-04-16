using FluentValidation;

namespace CloudDocs.Application.Features.Clients.UpdateClient;

public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Client name is required.")
            .MaximumLength(150)
                .WithMessage("Client name cannot exceed 150 characters.");

        RuleFor(x => x.LegalName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalName))
                .WithMessage("Legal name cannot exceed 200 characters.");

        RuleFor(x => x.Identification)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Identification))
                .WithMessage("Identification cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("A valid email address is required.")
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone cannot exceed 50 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
                .WithMessage("Notes cannot exceed 500 characters.");
    }
}