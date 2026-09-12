namespace CleanMinimalApi.Application.BackgroundJobs;

public sealed record OcrExtractionJobPayload(Guid EvidenceId);
public sealed record ReportGenerationJobPayload(Guid BillingPeriodId);
public sealed record AnomalyAnalysisJobPayload(Guid BillingPeriodId);
public sealed record ExportJobPayload(Guid BillingPeriodId, Guid JobId);
