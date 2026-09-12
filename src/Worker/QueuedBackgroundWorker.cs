namespace CleanMinimalApi.Worker;

using System.Threading;
using System.Threading.Tasks;
using CleanMinimalApi.Application.BackgroundJobs;
using Microsoft.Extensions.Hosting;

public sealed class QueuedBackgroundWorker(IBackgroundJobQueue jobQueue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobQueue.DequeueAsync(stoppingToken))
        {
            await job.WorkItem(stoppingToken);
        }
    }
}
