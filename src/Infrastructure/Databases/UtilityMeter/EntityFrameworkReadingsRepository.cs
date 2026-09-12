namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter;

using Application.Readings;
using Application.Readings.Entities;
using Microsoft.EntityFrameworkCore;

internal class EntityFrameworkReadingsRepository(UtilityMeterDbContext context) : IReadingsRepository
{
    private readonly UtilityMeterDbContext context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<Reading>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await this.context.Readings.OrderByDescending(r => r.MeasuredAt).ToListAsync(cancellationToken);
    }

    public async Task<Reading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Reading>> GetByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings
            .Where(r => r.MeterId == meterId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetByBillingPeriodIdAsync(Guid billingPeriodId, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings
            .Where(r => r.BillingPeriodId == billingPeriodId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reading?> GetLatestByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings
            .Where(r => r.MeterId == meterId)
            .OrderByDescending(r => r.MeasuredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Reading?> GetPreviousByMeterAsync(
        Guid meterId,
        DateTimeOffset measuredAt,
        Guid? billingPeriodId,
        CancellationToken cancellationToken = default)
    {
        var query = this.context.Readings
            .Where(reading => reading.MeterId == meterId && reading.MeasuredAt < measuredAt);

        if (billingPeriodId.HasValue)
        {
            query = query.Where(reading => reading.BillingPeriodId != billingPeriodId);
        }

        return await query
            .OrderByDescending(reading => reading.MeasuredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Reading reading, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reading);
        _ = await this.context.Readings.AddAsync(reading, cancellationToken);
        _ = await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Reading reading, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reading);
        this.context.Readings.Update(reading);
        _ = await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Readings.AnyAsync(r => r.Id == id, cancellationToken);
    }
}
