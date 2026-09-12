namespace CleanMinimalApi.Application.Reports;

using CleanMinimalApi.Application.Condominiums.Entities;
using CleanMinimalApi.Application.Meters.Entities;
using CleanMinimalApi.Application.Properties.Entities;
using CleanMinimalApi.Application.Readings.Entities;

public sealed record CondominiumReconciliationData(
    Condominium Condominium,
    IReadOnlyList<UtilityProperty> Properties,
    IReadOnlyList<Meter> Meters,
    IReadOnlyList<Reading> Readings);

public interface ICondominiumReconciliationRepository
{
    Task<CondominiumReconciliationData?> GetAsync(Guid condominiumId, Guid billingPeriodId, CancellationToken cancellationToken = default);
}

public sealed record CondominiumPropertyIssue(
    Guid PropertyId,
    string PropertyName,
    decimal? Consumption,
    IReadOnlyList<string> Reasons);

public sealed record CondominiumReconciliationResult(
    Guid CondominiumId,
    Guid BillingPeriodId,
    decimal? MainConsumption,
    decimal SubMeterConsumption,
    decimal Difference,
    decimal? DifferencePercentage,
    IReadOnlyList<CondominiumPropertyIssue> Suspects,
    string Explanation);

public static class CondominiumReconciliationCalculator
{
    public static CondominiumReconciliationResult Calculate(
        CondominiumReconciliationData data,
        Guid billingPeriodId)
    {
        ArgumentNullException.ThrowIfNull(data);

        var metersById = data.Meters.ToDictionary(meter => meter.Id);
        var readingsById = data.Readings.ToDictionary(reading => reading.Id);
        var periodReadings = data.Readings
            .Where(reading => reading.BillingPeriodId == billingPeriodId)
            .GroupBy(reading => reading.MeterId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(reading => reading.MeasuredAt).First());

        decimal? ConsumptionFor(Guid meterId)
        {
            if (!periodReadings.TryGetValue(meterId, out var reading))
            {
                return null;
            }

            return reading.PreviousReadingId.HasValue && readingsById.TryGetValue(reading.PreviousReadingId.Value, out var previous)
                ? Reading.CalculateConsumption(previous, reading)
                : reading.Value;
        }

        var mainConsumption = data.Condominium.MainMeterId.HasValue
            ? ConsumptionFor(data.Condominium.MainMeterId.Value)
            : null;

        Common.Enums.ResourceType? mainMeterResource = data.Condominium.MainMeterId.HasValue
            && metersById.TryGetValue(data.Condominium.MainMeterId.Value, out var mainMeter)
            ? mainMeter.ResourceType
            : null;

        var propertyIssues = new List<CondominiumPropertyIssue>();
        decimal subMeterConsumption = 0m;

        foreach (var property in data.Properties.Where(property => property.CondominiumId == data.Condominium.Id))
        {
            var propertyMeter = property.MeterIds
                .Select(meterId => metersById.GetValueOrDefault(meterId))
                .FirstOrDefault(meter => meter != null && (!mainMeterResource.HasValue || meter.ResourceType == mainMeterResource));
            decimal? consumption = propertyMeter == null ? null : ConsumptionFor(propertyMeter.Id);
            if (consumption.HasValue)
            {
                subMeterConsumption += consumption.Value;
            }

            var reasons = new List<string>();
            if (propertyMeter == null || !consumption.HasValue)
            {
                reasons.Add("Missing reading");
            }

            var propertyReadings = data.Readings.Where(reading =>
                reading.BillingPeriodId == billingPeriodId && reading.PropertyId == property.Id).ToList();
            if (propertyReadings.Any(reading => reading.Alerts.Any(alert => alert.Type == Alerts.AlertType.AbnormallyHighConsumption)))
            {
                reasons.Add("High consumption");
            }

            var resident = propertyReadings.FirstOrDefault(reading => reading.Source.Equals("Resident", StringComparison.OrdinalIgnoreCase));
            var company = propertyReadings.FirstOrDefault(reading => reading.Source.Equals("Company", StringComparison.OrdinalIgnoreCase));
            if (resident != null && company != null && resident.Value != company.Value)
            {
                reasons.Add("Bill mismatch");
            }

            if (reasons.Count > 0)
            {
                propertyIssues.Add(new CondominiumPropertyIssue(property.Id, property.Name, consumption, reasons));
            }
        }

        var difference = mainConsumption.HasValue ? mainConsumption.Value - subMeterConsumption : 0m;
        decimal? differencePercentage = mainConsumption.HasValue && mainConsumption.Value != 0m
            ? Math.Abs(difference) / Math.Abs(mainConsumption.Value) * 100m
            : null;
        var explanation = mainConsumption.HasValue
            ? $"Main meter consumption is {mainConsumption.Value}; property sub-meters total {subMeterConsumption}; difference is {difference}."
            : "The main meter has no reading for this billing period.";

        return new CondominiumReconciliationResult(
            data.Condominium.Id,
            billingPeriodId,
            mainConsumption,
            subMeterConsumption,
            difference,
            differencePercentage,
            propertyIssues,
            explanation);
    }
}