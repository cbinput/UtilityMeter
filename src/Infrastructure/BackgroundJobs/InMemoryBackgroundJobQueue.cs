namespace CleanMinimalApi.Infrastructure.BackgroundJobs;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using Application.BackgroundJobs;

internal sealed class InMemoryBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<Guid> jobs = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private readonly string pendingPath;
    private readonly string processingPath;
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

    public InMemoryBackgroundJobQueue()
    {
        var rootPath = Environment.GetEnvironmentVariable("BACKGROUND_JOB_QUEUE_PATH")
            ?? "/tmp/utilitymeter-background-jobs";
        this.pendingPath = Path.Combine(rootPath, "pending");
        this.processingPath = Path.Combine(rootPath, "processing");

        Directory.CreateDirectory(this.pendingPath);
        Directory.CreateDirectory(this.processingPath);
        RecoverProcessingJobs(this.pendingPath, this.processingPath);
        QueuePersistedPendingJobs(this.pendingPath, this.jobs.Writer);
    }

    public async ValueTask QueueAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        var path = GetPath(this.pendingPath, job.Id);
        var json = JsonSerializer.Serialize(job, this.jsonOptions);
        await File.WriteAllTextAsync(path, json, cancellationToken);
        _ = this.jobs.Writer.TryWrite(job.Id);
    }

    public async IAsyncEnumerable<BackgroundJob> DequeueAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await this.jobs.Reader.WaitToReadAsync(cancellationToken))
        {
            while (this.jobs.Reader.TryRead(out var queuedJobId))
            {
                if (TryClaimPendingJob(this.pendingPath, this.processingPath, queuedJobId, this.jsonOptions, out var claimedFromChannel))
                {
                    yield return claimedFromChannel;
                    continue;
                }

                if (TryClaimNextPendingJob(this.pendingPath, this.processingPath, this.jsonOptions, out var claimedFromDisk))
                {
                    yield return claimedFromDisk;
                }
            }
        }
    }

    public Task CompleteAsync(Guid jobId, bool succeeded, CancellationToken cancellationToken = default)
    {
        var processingFile = GetPath(this.processingPath, jobId);
        if (!File.Exists(processingFile))
        {
            return Task.CompletedTask;
        }

        if (succeeded)
        {
            File.Delete(processingFile);
            return Task.CompletedTask;
        }

        var pendingFile = GetPath(this.pendingPath, jobId);
        if (File.Exists(pendingFile))
        {
            File.Delete(pendingFile);
        }

        File.Move(processingFile, pendingFile);
        _ = this.jobs.Writer.TryWrite(jobId);
        return Task.CompletedTask;
    }

    private static bool TryClaimNextPendingJob(string pendingPath, string processingPath, JsonSerializerOptions options, out BackgroundJob job)
    {
        foreach (var path in Directory.EnumerateFiles(pendingPath, "*.json", SearchOption.TopDirectoryOnly)
                     .OrderBy(file => file, StringComparer.Ordinal))
        {
            if (TryClaimPendingJobFile(path, processingPath, options, out var claimed))
            {
                job = claimed;
                return true;
            }
        }

        job = default!;
        return false;
    }

    private static bool TryClaimPendingJob(
        string pendingPath,
        string processingPath,
        Guid id,
        JsonSerializerOptions options,
        out BackgroundJob job)
    {
        var pendingFile = GetPath(pendingPath, id);
        if (!File.Exists(pendingFile))
        {
            job = default!;
            return false;
        }

        return TryClaimPendingJobFile(pendingFile, processingPath, options, out job);
    }

    private static bool TryClaimPendingJobFile(string pendingFile, string processingPath, JsonSerializerOptions options, out BackgroundJob job)
    {
        var processingFile = Path.Combine(processingPath, Path.GetFileName(pendingFile));
        try
        {
            File.Move(pendingFile, processingFile);
        }
        catch (IOException)
        {
            job = default!;
            return false;
        }

        var content = File.ReadAllText(processingFile);
        var deserialized = JsonSerializer.Deserialize<BackgroundJob>(content, options);
        if (deserialized == null)
        {
            File.Delete(processingFile);
            job = default!;
            return false;
        }

        job = deserialized;
        return true;
    }

    private static void RecoverProcessingJobs(string pendingPath, string processingPath)
    {
        foreach (var file in Directory.EnumerateFiles(processingPath, "*.json", SearchOption.TopDirectoryOnly))
        {
            var pendingFile = Path.Combine(pendingPath, Path.GetFileName(file));
            if (File.Exists(pendingFile))
            {
                File.Delete(pendingFile);
            }

            File.Move(file, pendingFile);
        }
    }

    private static void QueuePersistedPendingJobs(string pendingPath, ChannelWriter<Guid> writer)
    {
        foreach (var file in Directory.EnumerateFiles(pendingPath, "*.json", SearchOption.TopDirectoryOnly))
        {
            if (Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out var jobId))
            {
                _ = writer.TryWrite(jobId);
            }
        }
    }

    private static string GetPath(string directory, Guid id) => Path.Combine(directory, $"{id:N}.json");
}
