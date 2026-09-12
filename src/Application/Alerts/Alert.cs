namespace CleanMinimalApi.Application.Alerts;

public sealed class Alert
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public AlertType Type { get; init; }
    public AlertSeverity Severity { get; init; } = AlertSeverity.Info;
    public string Message { get; init; } = string.Empty;
    public Guid? MeterId { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ReadingId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
