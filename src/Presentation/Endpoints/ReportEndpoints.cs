namespace CleanMinimalApi.Presentation.Endpoints;

using CleanMinimalApi.Application.BackgroundJobs;
using CleanMinimalApi.Application.BackgroundJobs.Commands;
using CleanMinimalApi.Application.Reports.Dtos;
using CleanMinimalApi.Application.Reports.Queries.GetAbnormalReadings;
using CleanMinimalApi.Application.Reports.Queries.GetCondominiumReconciliation;
using CleanMinimalApi.Application.Reports.Queries.GetMonthlySummary;
using CleanMinimalApi.Application.Reports.Queries.GetPendingReadings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

public static class ReportEndpoints
{
    public static WebApplication MapReportEndpoints(this WebApplication app)
    {
        var root = app.MapGroup("/api/reports").WithTags("reports");

        _ = root.MapGet("/pending-readings", GetPendingReadings)
            .Produces<List<PendingReadingDto>>()
            .WithSummary("List readings still pending");
        _ = root.MapGet("/abnormal-readings", GetAbnormalReadings)
            .Produces<List<AbnormalReadingDto>>()
            .WithSummary("List readings with business-rule alerts");
        _ = root.MapGet("/monthly-summary/{billingPeriodId}", GetMonthlySummary)
            .Produces<MonthlySummaryDto>()
            .WithSummary("Summarize readings and resident-company mismatches");
        _ = root.MapGet("/condominiums/{condominiumId}/reconciliation/{billingPeriodId}", GetCondominiumReconciliation)
            .Produces<CondominiumReconciliationDto>()
            .WithSummary("Reconcile the main meter with property sub-meters");
        _ = root.MapPost("/monthly-summary/{billingPeriodId}/generate", QueueMonthlySummaryGeneration)
            .Produces<QueuedBackgroundJobResponse>(StatusCodes.Status202Accepted)
            .WithSummary("Queue monthly summary generation");
        _ = root.MapPost("/abnormal-readings/{billingPeriodId}/analyze", QueueAnomalyAnalysis)
            .Produces<QueuedBackgroundJobResponse>(StatusCodes.Status202Accepted)
            .WithSummary("Queue anomaly analysis");
        _ = root.MapPost("/exports/{billingPeriodId}", QueueBillingPeriodExport)
            .Produces<QueuedBackgroundJobResponse>(StatusCodes.Status202Accepted)
            .WithSummary("Queue report export");

        return app;
    }

    public static async Task<Ok<List<PendingReadingDto>>> GetPendingReadings(
        Guid? billingPeriodId,
        Guid? propertyId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetPendingReadingsQuery(billingPeriodId, propertyId), cancellationToken));
    }

    public static async Task<Ok<List<AbnormalReadingDto>>> GetAbnormalReadings(
        Guid? billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetAbnormalReadingsQuery(billingPeriodId), cancellationToken));
    }

    public static async Task<Ok<MonthlySummaryDto>> GetMonthlySummary(
        Guid billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetMonthlySummaryQuery(billingPeriodId), cancellationToken));
    }

    public static async Task<Ok<CondominiumReconciliationDto>> GetCondominiumReconciliation(
        Guid condominiumId,
        Guid billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(
            new GetCondominiumReconciliationQuery(condominiumId, billingPeriodId),
            cancellationToken));
    }

    public static async Task<Accepted<QueuedBackgroundJobResponse>> QueueMonthlySummaryGeneration(
        Guid billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new EnqueueReportGenerationJobCommand(billingPeriodId), cancellationToken);
        return TypedResults.Accepted(
            $"/api/reports/monthly-summary/{billingPeriodId}",
            new QueuedBackgroundJobResponse(BackgroundJobTypes.ReportGeneration, "queued"));
    }

    public static async Task<Accepted<QueuedBackgroundJobResponse>> QueueAnomalyAnalysis(
        Guid billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new EnqueueAnomalyAnalysisJobCommand(billingPeriodId), cancellationToken);
        return TypedResults.Accepted(
            $"/api/reports/abnormal-readings?billingPeriodId={billingPeriodId}",
            new QueuedBackgroundJobResponse(BackgroundJobTypes.AnomalyAnalysis, "queued"));
    }

    public static async Task<Accepted<QueuedBackgroundJobResponse>> QueueBillingPeriodExport(
        Guid billingPeriodId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new EnqueueExportJobCommand(billingPeriodId), cancellationToken);
        return TypedResults.Accepted(
            $"/api/reports/monthly-summary/{billingPeriodId}",
            new QueuedBackgroundJobResponse(BackgroundJobTypes.Export, "queued"));
    }
}

public sealed record QueuedBackgroundJobResponse(string JobType, string Status);