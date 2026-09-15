namespace CleanMinimalApi.Infrastructure.Storage;

using Application.Storage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

internal sealed class MinioObjectStorage(IMinioClient client, IOptions<MinioStorageOptions> options) : IObjectStorage
{
    private readonly IMinioClient client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly MinioStorageOptions options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var streamToUpload = await ToSeekableStreamAsync(content, cancellationToken);
        await using var disposableStream = content.CanSeek ? null : streamToUpload;
        streamToUpload.Seek(0, SeekOrigin.Begin);
        var size = streamToUpload.Length;

        await this.client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(this.options.BucketName)
                .WithObject(key)
                .WithStreamData(streamToUpload)
                .WithObjectSize(size)
                .WithContentType(contentType),
            cancellationToken);

        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await this.client.GetObjectAsync(getObjectArgs, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key);

        await this.client.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await this.GetMetadataAsync(key, cancellationToken) is not null;
    }

    public async Task<ObjectStorageMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var objectStat = await this.client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(this.options.BucketName)
                    .WithObject(key),
                cancellationToken);

            return ToMetadata(key, objectStat);
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    private static ObjectStorageMetadata ToMetadata(string key, ObjectStat objectStat)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal);
        foreach (var entry in objectStat.MetaData)
        {
            if (!string.IsNullOrWhiteSpace(entry.Key) && entry.Value is not null)
            {
                metadata[entry.Key] = entry.Value;
            }
        }

        return new ObjectStorageMetadata(
            key,
            objectStat.Size,
            objectStat.ContentType,
            objectStat.ETag,
            objectStat.LastModified == default ? null : new DateTimeOffset(objectStat.LastModified),
            metadata);
    }

    private static async Task<Stream> ToSeekableStreamAsync(Stream source, CancellationToken cancellationToken)
    {
        if (source.CanSeek)
        {
            return source;
        }

        var directory = Path.Combine(Path.GetTempPath(), "utilitymeter");
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $"minio-upload-{Guid.NewGuid():N}.tmp");
        var tempStream = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 81_920,
            FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        await source.CopyToAsync(tempStream, cancellationToken);
        tempStream.Seek(0, SeekOrigin.Begin);
        return tempStream;
    }
}
