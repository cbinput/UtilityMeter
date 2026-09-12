namespace CleanMinimalApi.Application.Tests.Unit.Reports;

using CleanMinimalApi.Application.Alerts;
using CleanMinimalApi.Application.Common.Enums;
using CleanMinimalApi.Application.Condominiums.Entities;
using CleanMinimalApi.Application.Meters.Entities;
using CleanMinimalApi.Application.Properties.Entities;
using CleanMinimalApi.Application.Reports;
using CleanMinimalApi.Application.Readings.Entities;
using Shouldly;
using Xunit;

public class CondominiumReconciliationTests
{
    [Fact]
    public void Calculate_ShouldReportExactMatch()
    {
        var periodId = Guid.NewGuid();
        var mainMeter = new Meter { ResourceType = ResourceType.Water };
        var property = new UtilityProperty { Name = "House 1", CondominiumId = Guid.NewGuid() };
        var subMeter = new Meter { PropertyId = property.Id, ResourceType = ResourceType.Water };
        property.MeterIds.Add(subMeter.Id);
        var data = CreateData(property.CondominiumId!.Value, property, mainMeter, subMeter,
            ReadingFor(mainMeter.Id, periodId, 100m, 150m),
            ReadingFor(subMeter.Id, periodId, 50m, 100m));

        var result = CondominiumReconciliationCalculator.Calculate(data, periodId);

        result.MainConsumption.ShouldBe(50m);
        result.SubMeterConsumption.ShouldBe(50m);
        result.Difference.ShouldBe(0m);
        result.DifferencePercentage.ShouldBe(0m);
        result.Suspects.ShouldBeEmpty();
    }

    [Fact]
    public void Calculate_ShouldReportMismatchAndHighConsumptionSuspect()
    {
        var periodId = Guid.NewGuid();
        var condominiumId = Guid.NewGuid();
        var mainMeter = new Meter { ResourceType = ResourceType.Water };
        var property = new UtilityProperty { Name = "House 1", CondominiumId = condominiumId };
        var subMeter = new Meter { PropertyId = property.Id, ResourceType = ResourceType.Water };
        property.MeterIds.Add(subMeter.Id);
        var current = ReadingFor(subMeter.Id, periodId, 20m, 100m);
        current.PropertyId = property.Id;
        current.Alerts.Add(new Alert { Type = AlertType.AbnormallyHighConsumption });
        var data = CreateData(condominiumId, property, mainMeter, subMeter,
            ReadingFor(mainMeter.Id, periodId, 100m, 200m), current);

        var result = CondominiumReconciliationCalculator.Calculate(data, periodId);

        result.Difference.ShouldBe(20m);
        result.DifferencePercentage.ShouldBe(20m);
        result.Suspects.Single().Reasons.ShouldContain("High consumption");
    }

    [Fact]
    public void Calculate_ShouldFlagMissingPropertyReading()
    {
        var periodId = Guid.NewGuid();
        var condominiumId = Guid.NewGuid();
        var mainMeter = new Meter { ResourceType = ResourceType.Water };
        var property = new UtilityProperty { Name = "House 1", CondominiumId = condominiumId };
        var subMeter = new Meter { PropertyId = property.Id, ResourceType = ResourceType.Water };
        property.MeterIds.Add(subMeter.Id);
        var data = CreateData(condominiumId, property, mainMeter, subMeter,
            ReadingFor(mainMeter.Id, periodId, 100m, 150m));

        var result = CondominiumReconciliationCalculator.Calculate(data, periodId);

        result.Suspects.Single().Reasons.ShouldContain("Missing reading");
        result.SubMeterConsumption.ShouldBe(0m);
    }

    [Fact]
    public void Calculate_ShouldGroupBillMismatch()
    {
        var periodId = Guid.NewGuid();
        var condominiumId = Guid.NewGuid();
        var mainMeter = new Meter { ResourceType = ResourceType.Water };
        var property = new UtilityProperty { Name = "House 1", CondominiumId = condominiumId };
        var subMeter = new Meter { PropertyId = property.Id, ResourceType = ResourceType.Water };
        property.MeterIds.Add(subMeter.Id);
        var resident = ReadingFor(subMeter.Id, periodId, 0m, 40m);
        resident.PropertyId = property.Id;
        resident.Source = "Resident";
        var company = ReadingFor(subMeter.Id, periodId, 0m, 50m);
        company.PropertyId = property.Id;
        company.Source = "Company";
        var data = CreateData(condominiumId, property, mainMeter, subMeter,
            ReadingFor(mainMeter.Id, periodId, 100m, 150m), resident, company);

        var result = CondominiumReconciliationCalculator.Calculate(data, periodId);

        result.Suspects.Single().Reasons.ShouldContain("Bill mismatch");
    }

    private static CondominiumReconciliationData CreateData(
        Guid condominiumId,
        UtilityProperty property,
        Meter mainMeter,
        Meter subMeter,
        params Reading[] readings)
    {
        return new CondominiumReconciliationData(
            new Condominium
            {
                Id = condominiumId,
                MainMeterId = mainMeter.Id,
                PropertyIds = [property.Id]
            },
            [property],
            [mainMeter, subMeter],
            readings);
    }

    private static Reading ReadingFor(Guid meterId, Guid billingPeriodId, decimal previousValue, decimal value)
    {
        return new Reading
        {
            MeterId = meterId,
            BillingPeriodId = billingPeriodId,
            Value = value - previousValue,
            PropertyId = null,
            MeasuredAt = DateTimeOffset.UtcNow
        };
    }
}