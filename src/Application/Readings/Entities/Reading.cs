namespace CleanMinimalApi.Application.Readings.Entities;

using CleanMinimalApi.Application.Alerts;

public class Reading
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MeterId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? BillingPeriodId { get; set; }
    public Guid? PreviousReadingId { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTimeOffset MeasuredAt { get; set; }
    public string Source { get; set; } = "Manual";
    public string Status { get; set; } = "Pending";
    public List<Guid> EvidenceIds { get; set; } = [];
    public List<Alert> Alerts { get; set; } = [];

    public static decimal CalculateConsumption(Reading previous, Reading current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        if (current.Value < previous.Value)
        {
            return 0m;
        }

        return current.Value - previous.Value;
    }

    public static List<Alert> GetAlerts(Reading previous, Reading current, decimal historicalAverage = 0m)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var alerts = new List<Alert>();
        if (current.Value < previous.Value)
        {
            alerts.Add(new Alert
            {
                Type = AlertType.ReadingDecreased,
                Severity = AlertSeverity.Warning,
                Message = $"Current reading is lower than the previous reading ({previous.Value} -> {current.Value}).",
                MeterId = current.MeterId,
                PropertyId = current.PropertyId,
                ReadingId = current.Id
            });
        }

        var consumption = CalculateConsumption(previous, current);
        if (historicalAverage > 0m && consumption > historicalAverage * 1.5m)
        {
            alerts.Add(new Alert
            {
                Type = AlertType.AbnormallyHighConsumption,
                Severity = AlertSeverity.Warning,
                Message = $"Consumption of {consumption} is more than 50% above the historical average of {historicalAverage}.",
                MeterId = current.MeterId,
                PropertyId = current.PropertyId,
                ReadingId = current.Id
            });
        }

        return alerts;
    }

    public static Alert? GetAlert(Reading previous, Reading current)
    {
        return GetAlerts(previous, current).FirstOrDefault();
    }
}
