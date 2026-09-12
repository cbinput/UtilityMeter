namespace CleanMinimalApi.Application.Meters.Entities;

using CleanMinimalApi.Application.Common.Enums;

public class Meter
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PropertyId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; } = ResourceType.Water;
    public string Unit { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; } = DateTimeOffset.UtcNow;
}
