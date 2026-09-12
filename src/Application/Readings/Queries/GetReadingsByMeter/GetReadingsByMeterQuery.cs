namespace CleanMinimalApi.Application.Readings.Queries.GetReadingsByMeter;

using CleanMinimalApi.Application.Readings.Entities;
using MediatR;

public sealed record GetReadingsByMeterQuery(Guid MeterId) : IRequest<List<Reading>>;

public sealed class GetReadingsByMeterHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetReadingsByMeterQuery, List<Reading>>
{
    private readonly IReadingsRepository _readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<List<Reading>> Handle(GetReadingsByMeterQuery request, CancellationToken cancellationToken)
    {
        return await _readingsRepository.GetByMeterIdAsync(request.MeterId, cancellationToken);
    }
}
