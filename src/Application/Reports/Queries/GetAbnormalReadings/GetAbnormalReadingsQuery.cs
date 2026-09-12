namespace CleanMinimalApi.Application.Reports.Queries.GetAbnormalReadings;

using CleanMinimalApi.Application.Readings;
using CleanMinimalApi.Application.Reports.Dtos;
using MediatR;

public sealed record GetAbnormalReadingsQuery(Guid? BillingPeriodId = null) : IRequest<List<AbnormalReadingDto>>;

public sealed class GetAbnormalReadingsHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetAbnormalReadingsQuery, List<AbnormalReadingDto>>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<List<AbnormalReadingDto>> Handle(GetAbnormalReadingsQuery request, CancellationToken cancellationToken)
    {
        var readings = request.BillingPeriodId.HasValue
            ? await this.readingsRepository.GetByBillingPeriodIdAsync(request.BillingPeriodId.Value, cancellationToken)
            : await this.readingsRepository.GetAllAsync(cancellationToken);

        return [.. readings
            .Where(reading => reading.Alerts.Count > 0)
            .Select(reading => new AbnormalReadingDto(reading.Id, reading.PropertyId, reading.MeterId, reading.BillingPeriodId, reading.Alerts))];
    }
}