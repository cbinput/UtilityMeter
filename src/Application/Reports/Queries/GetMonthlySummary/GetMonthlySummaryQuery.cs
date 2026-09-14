namespace CleanMinimalApi.Application.Reports.Queries.GetMonthlySummary;

using CleanMinimalApi.Application.Readings;
using CleanMinimalApi.Application.Reports.Dtos;
using MediatR;

public sealed record GetMonthlySummaryQuery(Guid BillingPeriodId) : IRequest<MonthlySummaryDto>;

public sealed class GetMonthlySummaryHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var readings = await this.readingsRepository.GetByBillingPeriodIdAsync(request.BillingPeriodId, cancellationToken);
        var groupedReadings = readings
            .GroupBy(reading => new
            {
                reading.MeterId,
                reading.PropertyId,
                Source = reading.Source.Trim().ToUpperInvariant()
            })
            .Select(group => group
                .OrderByDescending(reading => reading.MeasuredAt)
                .First())
            .ToList();
        var residentReadings = groupedReadings
            .Where(reading => reading.Source.Equals("Resident", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(reading => (reading.MeterId, reading.PropertyId));
        var companyReadings = groupedReadings
            .Where(reading => reading.Source.Equals("Company", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(reading => (reading.MeterId, reading.PropertyId));
        var matchedKeys = residentReadings.Keys.Intersect(companyReadings.Keys).ToList();
        var mismatchCount = matchedKeys.Count(key => residentReadings[key].Value != companyReadings[key].Value);
        var resident = residentReadings.Count > 0 ? residentReadings.Values.Sum(reading => reading.Value) : (decimal?)null;
        var company = companyReadings.Count > 0 ? companyReadings.Values.Sum(reading => reading.Value) : (decimal?)null;
        decimal? mismatchPercentage = resident.HasValue && company.HasValue && company.Value != 0m
            ? Math.Abs(resident.Value - company.Value) / company.Value * 100m
            : null;

        return new MonthlySummaryDto(
            request.BillingPeriodId,
            readings.Count,
            readings.Count(reading => reading.Status == "Pending"),
            readings.Count(reading => reading.Alerts.Count > 0),
            mismatchCount,
            resident,
            company,
            mismatchPercentage);
    }
}