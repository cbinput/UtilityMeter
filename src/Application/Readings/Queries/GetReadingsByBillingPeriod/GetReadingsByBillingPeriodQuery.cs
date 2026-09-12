namespace CleanMinimalApi.Application.Readings.Queries.GetReadingsByBillingPeriod;

using CleanMinimalApi.Application.Readings.Entities;
using MediatR;

public sealed record GetReadingsByBillingPeriodQuery(Guid BillingPeriodId) : IRequest<List<Reading>>;

public sealed class GetReadingsByBillingPeriodHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetReadingsByBillingPeriodQuery, List<Reading>>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<List<Reading>> Handle(GetReadingsByBillingPeriodQuery request, CancellationToken cancellationToken)
    {
        return await this.readingsRepository.GetByBillingPeriodIdAsync(request.BillingPeriodId, cancellationToken);
    }
}
