namespace CleanMinimalApi.Application.Storage;

public interface IObjectStorage
{
    public Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);
    public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);
    public Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    public Task<ObjectStorageMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default);
}

public sealed record ObjectStorageMetadata(
    string Key,
    long Size,
    string? ContentType,
    string? ETag,
    DateTimeOffset? LastModified,
    IReadOnlyDictionary<string, string> Metadata);
