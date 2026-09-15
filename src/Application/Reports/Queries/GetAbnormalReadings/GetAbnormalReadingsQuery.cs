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
        var readings = await this.readingsRepository.GetAbnormalAsync(request.BillingPeriodId, cancellationToken);

        return [.. readings
            .Select(reading => new AbnormalReadingDto(reading.Id, reading.PropertyId, reading.MeterId, reading.BillingPeriodId, reading.Alerts))];
    }
}