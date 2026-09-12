namespace CleanMinimalApi.Application.Evidence.Commands.AttachEvidenceToReading;

using FluentValidation;

public sealed class AttachEvidenceToReadingValidator : AbstractValidator<AttachEvidenceToReadingCommand>
{
    public AttachEvidenceToReadingValidator()
    {
        _ = this.RuleFor(x => x.ReadingId)
            .NotEmpty()
            .WithMessage("ReadingId is required");

        _ = this.RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage("EvidenceId is required");
    }
}
