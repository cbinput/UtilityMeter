namespace CleanMinimalApi.Application.BackgroundJobs;

#pragma warning disable CA1711

public interface IBackgroundJobQueue
{
    public ValueTask QueueAsync(BackgroundJob job, CancellationToken cancellationToken = default);

    public IAsyncEnumerable<BackgroundJob> DequeueAsync(CancellationToken cancellationToken);

    public Task CompleteAsync(Guid jobId, bool succeeded, CancellationToken cancellationToken = default);
}

#pragma warning restore CA1711

public sealed record BackgroundJob(Guid Id, string JobType, string Payload, DateTimeOffset EnqueuedAt);
