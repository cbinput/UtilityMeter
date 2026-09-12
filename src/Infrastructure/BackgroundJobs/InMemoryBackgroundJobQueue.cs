namespace CleanMinimalApi.Infrastructure.BackgroundJobs;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using Application.BackgroundJobs;

internal sealed class InMemoryBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<BackgroundJob> _jobs = Channel.CreateUnbounded<BackgroundJob>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ValueTask QueueAsync(string jobType, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobType);
        ArgumentNullException.ThrowIfNull(workItem);

        var job = new BackgroundJob(jobType, workItem);
        return _jobs.Writer.WriteAsync(job, cancellationToken);
    }

    public async IAsyncEnumerable<BackgroundJob> DequeueAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _jobs.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_jobs.Reader.TryRead(out var queuedJob))
            {
                yield return queuedJob;
            }
        }
    }
}
