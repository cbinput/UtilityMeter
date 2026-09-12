namespace CleanMinimalApi.Application.Evidence.Commands.AttachEvidenceToReading;

using CleanMinimalApi.Application.Common.Exceptions;
using CleanMinimalApi.Application.Readings;
using MediatR;
using Serilog;

public sealed class AttachEvidenceToReadingHandler(IReadingsRepository readingsRepository, IEvidenceRepository evidenceRepository, ILogger logger)
    : IRequestHandler<AttachEvidenceToReadingCommand, AttachEvidenceToReadingResponse>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));
    private readonly IEvidenceRepository evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<AttachEvidenceToReadingResponse> Handle(AttachEvidenceToReadingCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Attaching evidence {EvidenceId} to reading {ReadingId}", request.EvidenceId, request.ReadingId);

        // Verify reading exists
        var reading = await this.readingsRepository.GetByIdAsync(request.ReadingId, cancellationToken) ?? throw new NotFoundException($"Reading with id {request.ReadingId} not found");

        // Verify evidence exists
        var evidenceExists = await this.evidenceRepository.ExistsAsync(request.EvidenceId, cancellationToken);
        if (!evidenceExists)
        {
            throw new NotFoundException($"Evidence with id {request.EvidenceId} not found");
        }

        // Check if evidence is already attached
        if (reading.EvidenceIds.Contains(request.EvidenceId))
        {
            this.logger.Warning("Evidence {EvidenceId} is already attached to reading {ReadingId}", request.EvidenceId, request.ReadingId);
            return new AttachEvidenceToReadingResponse(request.ReadingId, request.EvidenceId, "Evidence already attached");
        }

        // Add evidence to reading
        reading.EvidenceIds.Add(request.EvidenceId);
        await this.readingsRepository.UpdateAsync(reading, cancellationToken);

        this.logger.Information("Evidence {EvidenceId} attached to reading {ReadingId}", request.EvidenceId, request.ReadingId);

        return new AttachEvidenceToReadingResponse(request.ReadingId, request.EvidenceId, "Evidence attached successfully");
    }
}
