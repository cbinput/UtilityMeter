namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Organizations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        _ = builder.HasKey(o => o.Id);

        _ = builder.Property(o => o.Id)
            .ValueGeneratedNever();

        _ = builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        _ = builder.Property(o => o.ContactEmail)
            .IsRequired(false)
            .HasMaxLength(250);

        _ = builder.Property(o => o.PropertyIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList(),
                ValueComparers.GuidListComparer)
            .IsRequired();
    }
}
