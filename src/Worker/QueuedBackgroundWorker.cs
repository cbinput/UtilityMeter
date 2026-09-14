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
                WorkerLogMessages.ProcessingBackgroundJob(logger, job.JobType);
                await ProcessAsync(job, sender, stoppingToken);
                await jobQueue.CompleteAsync(job.Id, succeeded: true, stoppingToken);
                WorkerLogMessages.FinishedBackgroundJob(logger, job.JobType);
            }
            catch (Exception ex)
            {
                await jobQueue.CompleteAsync(job.Id, succeeded: false, stoppingToken);
                WorkerLogMessages.BackgroundJobFailed(logger, job.JobType, ex);
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
            BackgroundJobTypes.Export => sender.Send(
                CreateExportCommand(job.Payload),
                cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported background job type '{job.JobType}'.")
        };
    }

    private static ProcessExportJobCommand CreateExportCommand(string payload)
    {
        var export = JsonSerializer.Deserialize<ExportJobPayload>(payload)
            ?? throw new InvalidOperationException("Background job payload was invalid.");
        return new ProcessExportJobCommand(export.BillingPeriodId, export.JobId);
    }
}
