namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Properties.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class UtilityPropertyConfiguration : IEntityTypeConfiguration<UtilityProperty>
{
    public void Configure(EntityTypeBuilder<UtilityProperty> builder)
    {
        _ = builder.HasKey(p => p.Id);

        _ = builder.Property(p => p.Id)
            .ValueGeneratedNever();

        _ = builder.Property(p => p.OrganizationId)
            .IsRequired(false);

        _ = builder.Property(p => p.CondominiumId)
            .IsRequired(false);

        _ = builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        _ = builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(500);

        _ = builder.Property(p => p.ResourceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        _ = builder.Property(p => p.MeterIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList(),
                ValueComparers.GuidListComparer)
            .IsRequired();

        _ = builder.HasIndex(p => p.OrganizationId);
        _ = builder.HasIndex(p => p.CondominiumId);
    }
}
