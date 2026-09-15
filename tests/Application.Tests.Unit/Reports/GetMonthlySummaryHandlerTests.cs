namespace CleanMinimalApi.Application.Tests.Unit.Reports;

using CleanMinimalApi.Application.Readings;
using CleanMinimalApi.Application.Readings.Entities;
using CleanMinimalApi.Application.Reports.Queries.GetMonthlySummary;
using NSubstitute;
using Shouldly;
using Xunit;

public class GetMonthlySummaryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCountMeterLevelMismatches_EvenWhenTotalsCancelOut()
    {
        var billingPeriodId = Guid.NewGuid();
        var meterOne = Guid.NewGuid();
        var meterTwo = Guid.NewGuid();
        var propertyOne = Guid.NewGuid();
        var propertyTwo = Guid.NewGuid();
        var repository = Substitute.For<IReadingsRepository>();

        repository.GetMonthlySummarySnapshotAsync(billingPeriodId, Arg.Any<CancellationToken>())
            .Returns(
                new MonthlySummarySnapshot(
                    billingPeriodId,
                    4,
                    0,
                    0,
                    [
                        new MonthlySummarySnapshotReading(meterOne, propertyOne, "Resident", 100m, DateTimeOffset.UtcNow, Guid.NewGuid()),
                        new MonthlySummarySnapshotReading(meterOne, propertyOne, "Company", 110m, DateTimeOffset.UtcNow, Guid.NewGuid()),
                        new MonthlySummarySnapshotReading(meterTwo, propertyTwo, "Resident", 200m, DateTimeOffset.UtcNow, Guid.NewGuid()),
                        new MonthlySummarySnapshotReading(meterTwo, propertyTwo, "Company", 190m, DateTimeOffset.UtcNow, Guid.NewGuid())
                    ]));

        var handler = new GetMonthlySummaryHandler(repository);

        var result = await handler.Handle(new GetMonthlySummaryQuery(billingPeriodId), CancellationToken.None);

        result.MismatchCount.ShouldBe(2);
        result.ResidentTotal.ShouldBe(300m);
        result.CompanyTotal.ShouldBe(300m);
        result.MismatchPercentage.ShouldBe(0m);
    }
}
