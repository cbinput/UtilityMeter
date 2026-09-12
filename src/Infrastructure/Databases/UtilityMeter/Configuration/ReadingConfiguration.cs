namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Readings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class ReadingConfiguration : IEntityTypeConfiguration<Reading>
{
    public void Configure(EntityTypeBuilder<Reading> builder)
    {
        _ = builder.HasKey(r => r.Id);

        _ = builder.Property(r => r.Id)
            .ValueGeneratedNever();

        _ = builder.Property(r => r.MeterId)
            .IsRequired();

        _ = builder.Property(r => r.PropertyId)
            .IsRequired(false);

        _ = builder.Property(r => r.BillingPeriodId)
            .IsRequired(false);

        _ = builder.Property(r => r.PreviousReadingId)
            .IsRequired(false);

        _ = builder.Property(r => r.Value)
            .IsRequired()
            .HasPrecision(18, 2);

        _ = builder.Property(r => r.Unit)
            .IsRequired()
            .HasMaxLength(50);

        _ = builder.Property(r => r.MeasuredAt)
            .IsRequired();

        _ = builder.Property(r => r.Source)
            .IsRequired()
            .HasMaxLength(50);

        _ = builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50);

        _ = builder.Property(r => r.EvidenceIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList(),
                ValueComparers.GuidListComparer)
            .IsRequired();

        _ = builder.HasIndex(r => r.MeterId);
        _ = builder.HasIndex(r => r.PropertyId);
        _ = builder.HasIndex(r => r.BillingPeriodId);
    }
}
