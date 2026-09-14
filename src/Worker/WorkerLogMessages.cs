namespace CleanMinimalApi.Worker;

using Microsoft.Extensions.Logging;

internal static partial class WorkerLogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Processing background job {JobType}")]
    public static partial void ProcessingBackgroundJob(ILogger logger, string jobType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Finished background job {JobType}")]
    public static partial void FinishedBackgroundJob(ILogger logger, string jobType);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Background job {JobType} failed")]
    public static partial void BackgroundJobFailed(ILogger logger, string jobType, Exception exception);
}
