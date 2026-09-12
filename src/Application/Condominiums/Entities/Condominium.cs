namespace CleanMinimalApi.Application.Condominiums.Entities;

public class Condominium
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<Guid> PropertyIds { get; set; } = new();
    public Guid? MainMeterId { get; set; }
}
