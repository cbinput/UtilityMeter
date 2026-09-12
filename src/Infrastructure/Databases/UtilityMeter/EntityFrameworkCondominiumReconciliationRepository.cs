namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter;

using Application.Condominiums.Entities;
using Application.Reports;
using Microsoft.EntityFrameworkCore;

internal sealed class EntityFrameworkCondominiumReconciliationRepository(UtilityMeterDbContext context)
    : ICondominiumReconciliationRepository
{
    private readonly UtilityMeterDbContext context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<CondominiumReconciliationData?> GetAsync(
        Guid condominiumId,
        Guid billingPeriodId,
        CancellationToken cancellationToken = default)
    {
        var condominium = await this.context.Condominiums.FirstOrDefaultAsync(item => item.Id == condominiumId, cancellationToken);
        if (condominium == null)
        {
            return null;
        }

        var properties = await this.context.Properties
            .Where(property => property.CondominiumId == condominiumId || condominium.PropertyIds.Contains(property.Id))
            .ToListAsync(cancellationToken);
        var meterIds = properties.SelectMany(property => property.MeterIds)
            .Append(condominium.MainMeterId ?? Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var meters = await this.context.Meters.Where(meter => meterIds.Contains(meter.Id)).ToListAsync(cancellationToken);
        var readings = await this.context.Readings.Where(reading => meterIds.Contains(reading.MeterId)).ToListAsync(cancellationToken);

        return new CondominiumReconciliationData(condominium, properties, meters, readings);
    }
}