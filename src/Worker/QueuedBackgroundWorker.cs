namespace CleanMinimalApi.Worker;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CleanMinimalApi.Application.BackgroundJobs;
using CleanMinimalApi.Application.BackgroundJobs.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;

public sealed class QueuedBackgroundWorker(IBackgroundJobQueue jobQueue, ISender sender, ILogger<QueuedBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobQueue.DequeueAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Processing background job {JobType}", job.JobType);
                await ProcessAsync(job, sender, stoppingToken);
                await jobQueue.CompleteAsync(job.Id, succeeded: true, stoppingToken);
                logger.LogInformation("Finished background job {JobType}", job.JobType);
            }
            catch (Exception ex)
            {
                await jobQueue.CompleteAsync(job.Id, succeeded: false, stoppingToken);
                logger.LogError(ex, "Background job {JobType} failed", job.JobType);
            }
        }
    }

    private static Task ProcessAsync(BackgroundJob job, ISender sender, CancellationToken cancellationToken)
    {
        static T DeserializePayload<T>(string payload)
            where T : class
            => JsonSerializer.Deserialize<T>(payload) ?? throw new InvalidOperationException("Background job payload was invalid.");

        return job.JobType switch
        {
            BackgroundJobTypes.OcrExtraction => sender.Send(
                new ProcessOcrExtractionJobCommand(DeserializePayload<OcrExtractionJobPayload>(job.Payload).EvidenceId),
                cancellationToken),
            BackgroundJobTypes.ReportGeneration => sender.Send(
                new ProcessReportGenerationJobCommand(DeserializePayload<ReportGenerationJobPayload>(job.Payload).BillingPeriodId),
                cancellationToken),
            BackgroundJobTypes.AnomalyAnalysis => sender.Send(
                new ProcessAnomalyAnalysisJobCommand(DeserializePayload<AnomalyAnalysisJobPayload>(job.Payload).BillingPeriodId),
                cancellationToken),
            BackgroundJobTypes.Export => ProcessExportAsync(job.Payload, sender, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported background job type '{job.JobType}'.")
        };
    }

    private static Task ProcessExportAsync(string payload, ISender sender, CancellationToken cancellationToken)
    {
        var export = JsonSerializer.Deserialize<ExportJobPayload>(payload)
            ?? throw new InvalidOperationException("Background job payload was invalid.");
        return sender.Send(new ProcessExportJobCommand(export.BillingPeriodId, export.JobId), cancellationToken);
    }
}
