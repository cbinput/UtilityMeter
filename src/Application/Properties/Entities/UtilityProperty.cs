namespace CleanMinimalApi.Application.Properties.Entities;

using CleanMinimalApi.Application.Common.Enums;

public class UtilityProperty
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? OrganizationId { get; set; }
    public Guid? CondominiumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; } = ResourceType.Water;
    public List<Guid> MeterIds { get; set; } = [];
}
