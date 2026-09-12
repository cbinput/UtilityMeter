namespace CleanMinimalApi.Application.Readings.Commands.CreateReading;

using MediatR;

public sealed record CreateReadingCommand(
    Guid MeterId,
    Guid? PropertyId,
    Guid? BillingPeriodId,
    Guid? PreviousReadingId,
    decimal Value,
    string Unit,
    DateTimeOffset MeasuredAt,
    string Source = "Manual") : IRequest<CreateReadingResponse>;

public sealed record CreateReadingResponse(Guid Id, decimal Value, string Status);
