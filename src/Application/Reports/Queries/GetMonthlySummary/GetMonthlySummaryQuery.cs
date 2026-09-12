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
        var resident = readings.Where(reading => reading.Source.Equals("Resident", StringComparison.OrdinalIgnoreCase)).Sum(reading => reading.Value);
        var company = readings.Where(reading => reading.Source.Equals("Company", StringComparison.OrdinalIgnoreCase)).Sum(reading => reading.Value);
        var hasResident = readings.Any(reading => reading.Source.Equals("Resident", StringComparison.OrdinalIgnoreCase));
        var hasCompany = readings.Any(reading => reading.Source.Equals("Company", StringComparison.OrdinalIgnoreCase));
        var mismatch = hasResident && hasCompany && resident != company;
        decimal? mismatchPercentage = mismatch && company != 0m ? Math.Abs(resident - company) / company * 100m : null;

        return new MonthlySummaryDto(
            request.BillingPeriodId,
            readings.Count,
            readings.Count(reading => reading.Status == "Pending"),
            readings.Count(reading => reading.Alerts.Count > 0),
            mismatch ? 1 : 0,
            hasResident ? resident : null,
            hasCompany ? company : null,
            mismatchPercentage);
    }
}