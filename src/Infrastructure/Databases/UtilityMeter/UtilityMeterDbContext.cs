namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter;

using System.Reflection;
using Application.BillingPeriods.Entities;
using Application.Condominiums.Entities;
using Application.Evidence.Entities;
using Application.Meters.Entities;
using Application.Organizations.Entities;
using Application.Properties.Entities;
using Application.Readings.Entities;
using Application.Reports;
using Microsoft.EntityFrameworkCore;

internal class UtilityMeterDbContext(DbContextOptions<UtilityMeterDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Condominium> Condominiums { get; set; }
    public DbSet<UtilityProperty> Properties { get; set; }
    public DbSet<Meter> Meters { get; set; }
    public DbSet<BillingPeriod> BillingPeriods { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Evidence> Evidence { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
