namespace CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;

using FluentValidation;

public sealed class UploadEvidenceValidator : AbstractValidator<UploadEvidenceCommand>
{
    public UploadEvidenceValidator()
    {
        _ = this.RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("FileName is required")
            .MaximumLength(500)
            .WithMessage("FileName must not exceed 500 characters");

        _ = this.RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("ContentType is required")
            .MaximumLength(100)
            .WithMessage("ContentType must not exceed 100 characters");

        _ = this.RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("Content stream is required");
    }
}
