namespace CleanMinimalApi.Presentation.Requests;

public sealed record CreateReadingRequest(
    Guid MeterId,
    Guid? PropertyId,
    Guid? BillingPeriodId,
    Guid? PreviousReadingId,
    decimal Value,
    string Unit,
    DateTimeOffset MeasuredAt,
    string Source = "Manual");
