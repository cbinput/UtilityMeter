namespace CleanMinimalApi.Infrastructure.Storage;

internal sealed class MinioStorageOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "utility-meter-evidence";
    public bool UseSsl { get; set; }
}
