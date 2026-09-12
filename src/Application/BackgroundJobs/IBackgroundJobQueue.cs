namespace CleanMinimalApi.Application.BackgroundJobs;

#pragma warning disable CA1711

public interface IBackgroundJobQueue
{
    public ValueTask QueueAsync(string jobType, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default);

    public IAsyncEnumerable<BackgroundJob> DequeueAsync(CancellationToken cancellationToken);
}

#pragma warning restore CA1711

public sealed record BackgroundJob(string JobType, Func<CancellationToken, Task> WorkItem);
