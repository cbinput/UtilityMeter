namespace CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;

using MediatR;

public sealed record UploadEvidenceCommand(
    string FileName,
    string ContentType,
    Stream Content) : IRequest<UploadEvidenceResponse>;

public sealed record UploadEvidenceResponse(Guid Id, string FileName, string StorageKey, string Hash);
