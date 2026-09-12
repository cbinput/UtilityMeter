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

        repository.GetByBillingPeriodIdAsync(billingPeriodId, Arg.Any<CancellationToken>())
            .Returns(
            [
                new Reading { MeterId = meterOne, BillingPeriodId = billingPeriodId, PropertyId = propertyOne, Source = "Resident", Value = 100m, MeasuredAt = DateTimeOffset.UtcNow },
                new Reading { MeterId = meterOne, BillingPeriodId = billingPeriodId, PropertyId = propertyOne, Source = "Company", Value = 110m, MeasuredAt = DateTimeOffset.UtcNow },
                new Reading { MeterId = meterTwo, BillingPeriodId = billingPeriodId, PropertyId = propertyTwo, Source = "Resident", Value = 200m, MeasuredAt = DateTimeOffset.UtcNow },
                new Reading { MeterId = meterTwo, BillingPeriodId = billingPeriodId, PropertyId = propertyTwo, Source = "Company", Value = 190m, MeasuredAt = DateTimeOffset.UtcNow }
            ]);

        var handler = new GetMonthlySummaryHandler(repository);

        var result = await handler.Handle(new GetMonthlySummaryQuery(billingPeriodId), CancellationToken.None);

        result.MismatchCount.ShouldBe(2);
        result.ResidentTotal.ShouldBe(300m);
        result.CompanyTotal.ShouldBe(300m);
        result.MismatchPercentage.ShouldBe(0m);
    }
}
