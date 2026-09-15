namespace CleanMinimalApi.Infrastructure.Storage;

using System.Collections.Concurrent;
using System.IO;
using Application.Storage;

internal sealed class InMemoryObjectStorage : IObjectStorage
{
    private readonly ConcurrentDictionary<string, StoredObject> _blobs = new(StringComparer.Ordinal);

    public Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var ms = new MemoryStream();
        content.CopyTo(ms);
        _blobs[key] = new StoredObject(ms.ToArray(), contentType);

        return Task.FromResult(key);
    }

    public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_blobs.TryGetValue(key, out var storedObject))
        {
            throw new FileNotFoundException($"Object '{key}' was not found in storage.", key);
        }

        return Task.FromResult<Stream>(new MemoryStream(storedObject.Content, writable: false));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _blobs.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Task.FromResult(_blobs.ContainsKey(key));
    }

    public Task<ObjectStorageMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!_blobs.TryGetValue(key, out var storedObject))
        {
            return Task.FromResult<ObjectStorageMetadata?>(null);
        }

        return Task.FromResult<ObjectStorageMetadata?>(
            new ObjectStorageMetadata(
                key,
                storedObject.Content.LongLength,
                storedObject.ContentType,
                null,
                null,
                new Dictionary<string, string>(StringComparer.Ordinal)));
    }

    private sealed record StoredObject(byte[] Content, string ContentType);
}
