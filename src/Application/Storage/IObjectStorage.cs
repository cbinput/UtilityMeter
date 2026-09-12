namespace CleanMinimalApi.Application.Storage;

public interface IObjectStorage
{
    Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
