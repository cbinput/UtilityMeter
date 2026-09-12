namespace CleanMinimalApi.Application.Reports.Queries.GetPendingReadings;

using CleanMinimalApi.Application.Readings;
using CleanMinimalApi.Application.Reports.Dtos;
using MediatR;

public sealed record GetPendingReadingsQuery(Guid? BillingPeriodId = null, Guid? PropertyId = null) : IRequest<List<PendingReadingDto>>;

public sealed class GetPendingReadingsHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetPendingReadingsQuery, List<PendingReadingDto>>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<List<PendingReadingDto>> Handle(GetPendingReadingsQuery request, CancellationToken cancellationToken)
    {
        var readings = request.BillingPeriodId.HasValue
            ? await this.readingsRepository.GetByBillingPeriodIdAsync(request.BillingPeriodId.Value, cancellationToken)
            : request.PropertyId.HasValue
                ? await this.readingsRepository.GetByPropertyIdAsync(request.PropertyId.Value, cancellationToken)
                : await this.readingsRepository.GetAllAsync(cancellationToken);

        return [.. readings
            .Where(reading => reading.Status == "Pending" && (!request.PropertyId.HasValue || reading.PropertyId == request.PropertyId))
            .Select(reading => new PendingReadingDto(reading.Id, reading.PropertyId, reading.MeterId, reading.BillingPeriodId, reading.MeasuredAt))];
    }
}