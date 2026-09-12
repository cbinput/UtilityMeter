namespace CleanMinimalApi.Application.Consumption;

public sealed class Consumption
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MeterId { get; set; }
    public Guid ReadingId { get; set; }
    public Guid? PreviousReadingId { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; init; } = DateTimeOffset.UtcNow;
    public static bool IsDerived => true;
}
