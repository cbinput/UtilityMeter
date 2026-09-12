namespace CleanMinimalApi.Application.BackgroundJobs.Commands;

using System.Text.Json;
using CleanMinimalApi.Application.Evidence;
using CleanMinimalApi.Application.Reports.Queries.GetAbnormalReadings;
using CleanMinimalApi.Application.Reports.Queries.GetMonthlySummary;
using CleanMinimalApi.Application.Storage;
using MediatR;
using Serilog;

public sealed record EnqueueOcrExtractionJobCommand(Guid EvidenceId) : IRequest;
public sealed record EnqueueReportGenerationJobCommand(Guid BillingPeriodId) : IRequest;
public sealed record EnqueueAnomalyAnalysisJobCommand(Guid BillingPeriodId) : IRequest;
public sealed record EnqueueExportJobCommand(Guid BillingPeriodId) : IRequest;

public sealed record ProcessOcrExtractionJobCommand(Guid EvidenceId) : IRequest;
public sealed record ProcessReportGenerationJobCommand(Guid BillingPeriodId) : IRequest;
public sealed record ProcessAnomalyAnalysisJobCommand(Guid BillingPeriodId) : IRequest;
public sealed record ProcessExportJobCommand(Guid BillingPeriodId) : IRequest;

public sealed class EnqueueOcrExtractionJobHandler(IBackgroundJobQueue queue, ISender sender, ILogger logger)
    : IRequestHandler<EnqueueOcrExtractionJobCommand>
{
    private readonly IBackgroundJobQueue queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(EnqueueOcrExtractionJobCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Queueing OCR extraction job for evidence {EvidenceId}", request.EvidenceId);

        await this.queue.QueueAsync(
            BackgroundJobTypes.OcrExtraction,
            token => this.sender.Send(new ProcessOcrExtractionJobCommand(request.EvidenceId), token),
            cancellationToken);

        return Unit.Value;
    }
}

public sealed class EnqueueReportGenerationJobHandler(IBackgroundJobQueue queue, ISender sender, ILogger logger)
    : IRequestHandler<EnqueueReportGenerationJobCommand>
{
    private readonly IBackgroundJobQueue queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(EnqueueReportGenerationJobCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Queueing report generation job for billing period {BillingPeriodId}", request.BillingPeriodId);

        await this.queue.QueueAsync(
            BackgroundJobTypes.ReportGeneration,
            token => this.sender.Send(new ProcessReportGenerationJobCommand(request.BillingPeriodId), token),
            cancellationToken);

        return Unit.Value;
    }
}

public sealed class EnqueueAnomalyAnalysisJobHandler(IBackgroundJobQueue queue, ISender sender, ILogger logger)
    : IRequestHandler<EnqueueAnomalyAnalysisJobCommand>
{
    private readonly IBackgroundJobQueue queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(EnqueueAnomalyAnalysisJobCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Queueing anomaly analysis job for billing period {BillingPeriodId}", request.BillingPeriodId);

        await this.queue.QueueAsync(
            BackgroundJobTypes.AnomalyAnalysis,
            token => this.sender.Send(new ProcessAnomalyAnalysisJobCommand(request.BillingPeriodId), token),
            cancellationToken);

        return Unit.Value;
    }
}

public sealed class EnqueueExportJobHandler(IBackgroundJobQueue queue, ISender sender, ILogger logger)
    : IRequestHandler<EnqueueExportJobCommand>
{
    private readonly IBackgroundJobQueue queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(EnqueueExportJobCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Queueing export job for billing period {BillingPeriodId}", request.BillingPeriodId);

        await this.queue.QueueAsync(
            BackgroundJobTypes.Export,
            token => this.sender.Send(new ProcessExportJobCommand(request.BillingPeriodId), token),
            cancellationToken);

        return Unit.Value;
    }
}

public sealed class ProcessOcrExtractionJobHandler(IEvidenceRepository evidenceRepository, ILogger logger)
    : IRequestHandler<ProcessOcrExtractionJobCommand>
{
    private readonly IEvidenceRepository evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(ProcessOcrExtractionJobCommand request, CancellationToken cancellationToken)
    {
        var evidence = await this.evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        if (evidence == null)
        {
            this.logger.Warning("Skipping OCR extraction because evidence {EvidenceId} was not found", request.EvidenceId);
            return Unit.Value;
        }

        this.logger.Information("Processed OCR extraction job for evidence {EvidenceId} and storage key {StorageKey}", evidence.Id, evidence.StorageKey);
        return Unit.Value;
    }
}

public sealed class ProcessReportGenerationJobHandler(ISender sender, ILogger logger)
    : IRequestHandler<ProcessReportGenerationJobCommand>
{
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(ProcessReportGenerationJobCommand request, CancellationToken cancellationToken)
    {
        var summary = await this.sender.Send(new GetMonthlySummaryQuery(request.BillingPeriodId), cancellationToken);
        this.logger.Information("Processed report generation job for billing period {BillingPeriodId} with {ReadingCount} readings", request.BillingPeriodId, summary.ReadingCount);
        return Unit.Value;
    }
}

public sealed class ProcessAnomalyAnalysisJobHandler(ISender sender, ILogger logger)
    : IRequestHandler<ProcessAnomalyAnalysisJobCommand>
{
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(ProcessAnomalyAnalysisJobCommand request, CancellationToken cancellationToken)
    {
        var abnormalities = await this.sender.Send(new GetAbnormalReadingsQuery(request.BillingPeriodId), cancellationToken);
        this.logger.Information("Processed anomaly analysis job for billing period {BillingPeriodId} and found {Count} abnormal readings", request.BillingPeriodId, abnormalities.Count);
        return Unit.Value;
    }
}

public sealed class ProcessExportJobHandler(ISender sender, IObjectStorage objectStorage, ILogger logger)
    : IRequestHandler<ProcessExportJobCommand>
{
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly IObjectStorage objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Unit> Handle(ProcessExportJobCommand request, CancellationToken cancellationToken)
    {
        var summary = await this.sender.Send(new GetMonthlySummaryQuery(request.BillingPeriodId), cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(summary);
        await using var stream = new MemoryStream(payload);
        var key = $"exports/{request.BillingPeriodId}/monthly-summary.json";
        var uploadedKey = await this.objectStorage.UploadAsync(stream, key, "application/json", cancellationToken);

        this.logger.Information("Processed export job for billing period {BillingPeriodId} and stored output at {StorageKey}", request.BillingPeriodId, uploadedKey);
        return Unit.Value;
    }
}
