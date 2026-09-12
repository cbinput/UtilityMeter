namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Condominiums.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;

internal class CondominiumConfiguration : IEntityTypeConfiguration<Condominium>
{
    public void Configure(EntityTypeBuilder<Condominium> builder)
    {
        _ = builder.HasKey(c => c.Id);

        _ = builder.Property(c => c.Id)
            .ValueGeneratedNever();

        _ = builder.Property(c => c.OrganizationId)
            .IsRequired(false);

        _ = builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        _ = builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        _ = builder.Property(c => c.PropertyIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList(),
                ValueComparers.GuidListComparer)
            .IsRequired();

        _ = builder.Property(c => c.MainMeterId)
            .IsRequired(false);

        _ = builder.HasIndex(c => c.OrganizationId);
    }
}
