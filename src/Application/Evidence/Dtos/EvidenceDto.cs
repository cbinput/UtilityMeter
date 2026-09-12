namespace CleanMinimalApi.Application.Evidence.Dtos;

public sealed record EvidenceDto(
    Guid Id,
    string FileName,
    string StorageKey,
    string ContentType,
    string Hash,
    Dictionary<string, string> Metadata);
