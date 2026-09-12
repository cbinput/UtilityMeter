namespace CleanMinimalApi.Application.Readings.Queries.GetReadingsByProperty;

using CleanMinimalApi.Application.Readings.Entities;
using MediatR;

public sealed record GetReadingsByPropertyQuery(Guid PropertyId) : IRequest<List<Reading>>;

public sealed class GetReadingsByPropertyHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetReadingsByPropertyQuery, List<Reading>>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<List<Reading>> Handle(GetReadingsByPropertyQuery request, CancellationToken cancellationToken)
    {
        return await this.readingsRepository.GetByPropertyIdAsync(request.PropertyId, cancellationToken);
    }
}
