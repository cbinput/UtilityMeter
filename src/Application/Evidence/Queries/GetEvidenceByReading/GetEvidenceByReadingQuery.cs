namespace CleanMinimalApi.Application.Evidence.Queries.GetEvidenceByReading;

using CleanMinimalApi.Application.Evidence.Entities;
using MediatR;

public sealed record GetEvidenceByReadingQuery(Guid ReadingId) : IRequest<List<Evidence>>;

public sealed class GetEvidenceByReadingHandler(IEvidenceRepository evidenceRepository)
    : IRequestHandler<GetEvidenceByReadingQuery, List<Evidence>>
{
    private readonly IEvidenceRepository evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));

    public async Task<List<Evidence>> Handle(GetEvidenceByReadingQuery request, CancellationToken cancellationToken)
    {
        return await this.evidenceRepository.GetByReadingIdAsync(request.ReadingId, cancellationToken);
    }
}
