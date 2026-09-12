namespace CleanMinimalApi.Application.Evidence.Entities;

public class Evidence
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string StorageKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string Hash { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public byte[]? RawContent { get; init; }
    public static bool IsImmutable => true;
}
