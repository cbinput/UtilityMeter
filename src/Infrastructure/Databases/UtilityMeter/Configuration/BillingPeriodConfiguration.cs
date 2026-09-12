namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.BillingPeriods.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class BillingPeriodConfiguration : IEntityTypeConfiguration<BillingPeriod>
{
    public void Configure(EntityTypeBuilder<BillingPeriod> builder)
    {
        _ = builder.HasKey(b => b.Id);

        _ = builder.Property(b => b.Id)
            .ValueGeneratedNever();

        _ = builder.Property(b => b.Start)
            .IsRequired();

        _ = builder.Property(b => b.End)
            .IsRequired();

        _ = builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        _ = builder.HasIndex(b => b.Start);
        _ = builder.HasIndex(b => b.End);
        _ = builder.HasIndex(b => b.Status);
    }
}
