namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Meters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class MeterConfiguration : IEntityTypeConfiguration<Meter>
{
    public void Configure(EntityTypeBuilder<Meter> builder)
    {
        _ = builder.HasKey(m => m.Id);

        _ = builder.Property(m => m.Id)
            .ValueGeneratedNever();

        _ = builder.Property(m => m.PropertyId)
            .IsRequired();

        _ = builder.Property(m => m.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        _ = builder.Property(m => m.ResourceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        _ = builder.Property(m => m.Unit)
            .IsRequired()
            .HasMaxLength(50);

        _ = builder.Property(m => m.Model)
            .IsRequired()
            .HasMaxLength(100);

        _ = builder.Property(m => m.InstalledAt)
            .IsRequired();

        _ = builder.HasIndex(m => m.PropertyId);
    }
}
