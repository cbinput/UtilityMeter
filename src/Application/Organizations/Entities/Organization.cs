namespace CleanMinimalApi.Application.Organizations.Entities;

public class Organization
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public List<Guid> PropertyIds { get; set; } = [];
}
