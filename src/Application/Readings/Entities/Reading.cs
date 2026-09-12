namespace CleanMinimalApi.Application.Readings.Entities;

using CleanMinimalApi.Application.Alerts;

public class Reading
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public decimal Value { get; set; }
    public DateTimeOffset MeasuredAt { get; set; }
    public string Source { get; set; } = "Manual";
    public string Status { get; set; } = "Pending";

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

    public static Alert? GetAlert(Reading previous, Reading current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        if (current.Value < previous.Value)
        {
            return new Alert
            {
                Severity = AlertSeverity.Warning,
                Message = $"Current reading is lower than the previous reading ({previous.Value} -> {current.Value})."
            };
        }

        return null;
    }
}
