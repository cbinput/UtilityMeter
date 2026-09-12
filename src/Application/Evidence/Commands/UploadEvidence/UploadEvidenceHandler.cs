namespace CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;

using System.Security.Cryptography;
using CleanMinimalApi.Application.BackgroundJobs.Commands;
using CleanMinimalApi.Application.Evidence.Entities;
using CleanMinimalApi.Application.Storage;
using MediatR;
using Serilog;

public sealed class UploadEvidenceHandler(IEvidenceRepository evidenceRepository, IObjectStorage objectStorage, ISender sender, ILogger logger)
    : IRequestHandler<UploadEvidenceCommand, UploadEvidenceResponse>
{
    private readonly IEvidenceRepository evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
    private readonly IObjectStorage objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
    private readonly ISender sender = sender ?? throw new ArgumentNullException(nameof(sender));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UploadEvidenceResponse> Handle(UploadEvidenceCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Uploading evidence file {FileName}", request.FileName);

        // Calculate hash of the content
        string hash;
        _ = request.Content.Seek(0, SeekOrigin.Begin);
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = await sha256.ComputeHashAsync(request.Content, cancellationToken);
            hash = Convert.ToHexString(hashBytes);
        }

        // Reset stream for upload
        request.Content.Seek(0, SeekOrigin.Begin);

        // Generate storage key
        var storageKey = $"evidence/{Guid.NewGuid()}/{request.FileName}";

        // Upload to object storage
        var uploadedKey = await this.objectStorage.UploadAsync(request.Content, storageKey, request.ContentType, cancellationToken);

        this.logger.Information("Evidence uploaded to storage key {StorageKey} with hash {Hash}", uploadedKey, hash);

        // Create evidence record
        var evidence = new Evidence
        {
            StorageKey = uploadedKey,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Hash = hash,
            Metadata = []
        };

        await this.evidenceRepository.AddAsync(evidence, cancellationToken);

        await this.sender.Send(new EnqueueOcrExtractionJobCommand(evidence.Id), cancellationToken);

        this.logger.Information("Evidence record created with id {EvidenceId}", evidence.Id);

        return new UploadEvidenceResponse(evidence.Id, evidence.FileName, evidence.StorageKey, evidence.Hash);
    }
}
