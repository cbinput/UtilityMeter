namespace CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;

using System.Buffers;
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

        var preparedUpload = await PrepareUploadAsync(request.Content, cancellationToken);
        await using var uploadStream = preparedUpload.Stream;

        // Generate storage key
        var storageKey = $"evidence/{Guid.NewGuid()}/{request.FileName}";

        // Upload to object storage
        var uploadedKey = await this.objectStorage.UploadAsync(uploadStream, storageKey, request.ContentType, cancellationToken);

        this.logger.Information("Evidence uploaded to storage key {StorageKey} with hash {Hash}", uploadedKey, preparedUpload.Hash);

        // Create evidence record
        var evidence = new Evidence
        {
            StorageKey = uploadedKey,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Hash = preparedUpload.Hash,
            Metadata = []
        };

        await this.evidenceRepository.AddAsync(evidence, cancellationToken);

        await this.sender.Send(new EnqueueOcrExtractionJobCommand(evidence.Id), cancellationToken);

        this.logger.Information("Evidence record created with id {EvidenceId}", evidence.Id);

        return new UploadEvidenceResponse(evidence.Id, evidence.FileName, evidence.StorageKey, evidence.Hash);
    }

    private static async Task<PreparedUpload> PrepareUploadAsync(Stream source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        var directory = Path.Combine(Path.GetTempPath(), "utilitymeter");
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $"evidence-upload-{Guid.NewGuid():N}.tmp");
        var tempStream = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 81_920,
            FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(81_920);
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                hash.AppendData(buffer, 0, bytesRead);
                await tempStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            tempStream.Seek(0, SeekOrigin.Begin);
            return new PreparedUpload(tempStream, Convert.ToHexString(hash.GetHashAndReset()));
        }
        catch
        {
            await tempStream.DisposeAsync();
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private sealed record PreparedUpload(FileStream Stream, string Hash);
}
