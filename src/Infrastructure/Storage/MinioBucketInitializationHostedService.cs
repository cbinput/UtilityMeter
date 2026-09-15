namespace CleanMinimalApi.Infrastructure.Storage;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

internal sealed class MinioBucketInitializationHostedService(IMinioClient client, IOptions<MinioStorageOptions> options) : IHostedService
{
    private readonly IMinioClient client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly MinioStorageOptions options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(this.options.BucketName);
        if (await this.client.BucketExistsAsync(bucketExistsArgs, cancellationToken))
        {
            return;
        }

        try
        {
            await this.client.MakeBucketAsync(new MakeBucketArgs().WithBucket(this.options.BucketName), cancellationToken);
        }
        catch (MinioException)
        {
            if (!await this.client.BucketExistsAsync(bucketExistsArgs, cancellationToken))
            {
                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
