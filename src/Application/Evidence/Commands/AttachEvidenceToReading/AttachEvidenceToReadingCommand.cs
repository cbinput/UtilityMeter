namespace CleanMinimalApi.Application.Evidence.Commands.AttachEvidenceToReading;

using MediatR;

public sealed record AttachEvidenceToReadingCommand(Guid ReadingId, Guid EvidenceId) : IRequest<AttachEvidenceToReadingResponse>;

public sealed record AttachEvidenceToReadingResponse(Guid ReadingId, Guid EvidenceId, string Message);
