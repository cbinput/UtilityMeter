namespace CleanMinimalApi.Application.Reports.Dtos;

public sealed record CondominiumPropertyIssueDto(
    Guid PropertyId,
    string PropertyName,
    decimal? Consumption,
    IReadOnlyList<string> Reasons);

public sealed record CondominiumReconciliationDto(
    Guid CondominiumId,
    Guid BillingPeriodId,
    decimal? MainConsumption,
    decimal SubMeterConsumption,
    decimal Difference,
    decimal? DifferencePercentage,
    IReadOnlyList<CondominiumPropertyIssueDto> Suspects,
    string Explanation);