namespace CleanMinimalApi.Application.Readings.Dtos;

using CleanMinimalApi.Application.Alerts;

public sealed record ReadingDto(
    Guid Id,
    Guid MeterId,
    Guid? PropertyId,
    Guid? BillingPeriodId,
    Guid? PreviousReadingId,
    decimal Value,
    string Unit,
    DateTimeOffset MeasuredAt,
    string Source,
    string Status,
    List<Guid> EvidenceIds,
    List<Alert> Alerts);
