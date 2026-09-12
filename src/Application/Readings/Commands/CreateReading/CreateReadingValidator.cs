namespace CleanMinimalApi.Application.Readings.Commands.CreateReading;

using FluentValidation;

public sealed class CreateReadingValidator : AbstractValidator<CreateReadingCommand>
{
    public CreateReadingValidator()
    {
        _ = this.RuleFor(x => x.MeterId)
            .NotEmpty()
            .WithMessage("MeterId is required");

        _ = this.RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Reading value must be non-negative");

        _ = this.RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("Unit is required")
            .MaximumLength(50)
            .WithMessage("Unit must not exceed 50 characters");

        _ = this.RuleFor(x => x.MeasuredAt)
            .NotEmpty()
            .WithMessage("MeasuredAt is required");

        _ = this.RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required")
            .MaximumLength(50)
            .WithMessage("Source must not exceed 50 characters");
    }
}
