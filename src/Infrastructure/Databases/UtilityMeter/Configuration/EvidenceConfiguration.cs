namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using Application.Evidence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        _ = builder.HasKey(e => e.Id);

        _ = builder.Property(e => e.Id)
            .ValueGeneratedNever();

        _ = builder.Property(e => e.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        _ = builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        _ = builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        _ = builder.Property(e => e.Hash)
            .IsRequired()
            .HasMaxLength(100);

        _ = builder.Property(e => e.Metadata)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new(),
                ValueComparers.StringDictionaryComparer)
            .IsRequired();

        _ = builder.Ignore(e => e.RawContent);
    }
}
