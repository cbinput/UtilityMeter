namespace CleanMinimalApi.Application.Alerts;

public sealed class Alert
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public AlertSeverity Severity { get; init; } = AlertSeverity.Info;
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
