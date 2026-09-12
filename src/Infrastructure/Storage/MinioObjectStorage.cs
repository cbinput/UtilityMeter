namespace CleanMinimalApi.Infrastructure.Storage;

using Application.Storage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

internal sealed class MinioObjectStorage(IOptions<MinioStorageOptions> options) : IObjectStorage
{
    private readonly MinioStorageOptions options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    private IMinioClient Client => new MinioClient()
        .WithEndpoint(this.options.Endpoint)
        .WithCredentials(this.options.AccessKey, this.options.SecretKey)
        .WithSSL(this.options.UseSsl)
        .Build();

    public async Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await this.EnsureBucketExistsAsync(cancellationToken);

        var streamToUpload = await ToSeekableStreamAsync(content, cancellationToken);
        var size = streamToUpload.Length - streamToUpload.Position;

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key)
            .WithStreamData(streamToUpload)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await this.Client.PutObjectAsync(putObjectArgs, cancellationToken);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await this.EnsureBucketExistsAsync(cancellationToken);

        var memoryStream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await this.Client.GetObjectAsync(getObjectArgs, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await this.EnsureBucketExistsAsync(cancellationToken);

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key);

        await this.Client.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await this.EnsureBucketExistsAsync(cancellationToken);

        var statObjectArgs = new StatObjectArgs()
            .WithBucket(this.options.BucketName)
            .WithObject(key);

        try
        {
            _ = await this.Client.StatObjectAsync(statObjectArgs, cancellationToken);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(this.options.BucketName);
        var bucketExists = await this.Client.BucketExistsAsync(bucketExistsArgs, cancellationToken);
        if (bucketExists)
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs().WithBucket(this.options.BucketName);
        await this.Client.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }

    private static async Task<Stream> ToSeekableStreamAsync(Stream source, CancellationToken cancellationToken)
    {
        if (source.CanSeek)
        {
            return source;
        }

        var memoryStream = new MemoryStream();
        await source.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
