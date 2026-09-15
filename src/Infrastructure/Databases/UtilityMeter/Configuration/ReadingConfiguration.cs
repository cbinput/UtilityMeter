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

        _ = builder.OwnsMany(r => r.Alerts, alerts =>
        {
            _ = alerts.WithOwner().HasForeignKey("ReadingId");
            _ = alerts.HasKey(alert => alert.Id);
            _ = alerts.Property(alert => alert.Message).IsRequired().HasMaxLength(500);
            _ = alerts.Property(alert => alert.Type).IsRequired();
            _ = alerts.Property(alert => alert.Severity).IsRequired();
            _ = alerts.Property(alert => alert.CreatedAt).IsRequired();
        });

        _ = builder.HasIndex(r => r.MeterId);
        _ = builder.HasIndex(r => r.PropertyId);
        _ = builder.HasIndex(r => r.BillingPeriodId);
        _ = builder.HasIndex(r => new { r.MeterId, r.MeasuredAt });
        _ = builder.HasIndex(r => new { r.BillingPeriodId, r.MeasuredAt });
        _ = builder.HasIndex(r => new { r.BillingPeriodId, r.Status });
        _ = builder.HasIndex(r => new { r.PropertyId, r.MeasuredAt });
    }
}
