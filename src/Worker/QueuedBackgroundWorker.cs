namespace CleanMinimalApi.Worker;

using System.Threading;
using System.Threading.Tasks;
using CleanMinimalApi.Application.BackgroundJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class QueuedBackgroundWorker(IBackgroundJobQueue jobQueue, ILogger<QueuedBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobQueue.DequeueAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Processing background job {JobType}", job.JobType);
                await job.WorkItem(stoppingToken);
                logger.LogInformation("Finished background job {JobType}", job.JobType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background job {JobType} failed", job.JobType);
            }
        }
    }
}
