namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter;

using Application.Readings;
using Application.Readings.Entities;
using Microsoft.EntityFrameworkCore;

internal class EntityFrameworkReadingsRepository(UtilityMeterDbContext context) : IReadingsRepository
{
    private readonly UtilityMeterDbContext context = context ?? throw new ArgumentNullException(nameof(context));
    private IQueryable<Reading> Readings => this.context.Readings.AsNoTracking();

    public async Task<List<Reading>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await this.Readings.OrderByDescending(r => r.MeasuredAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetPendingAsync(
        Guid? billingPeriodId = null,
        Guid? propertyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = this.Readings.Where(reading => reading.Status == "Pending");
        if (billingPeriodId.HasValue)
        {
            query = query.Where(reading => reading.BillingPeriodId == billingPeriodId.Value);
        }

        if (propertyId.HasValue)
        {
            query = query.Where(reading => reading.PropertyId == propertyId.Value);
        }

        return await query
            .OrderByDescending(reading => reading.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetAbnormalAsync(Guid? billingPeriodId = null, CancellationToken cancellationToken = default)
    {
        var query = this.context.Readings
            .AsNoTracking()
            .Include(reading => reading.Alerts)
            .Where(reading => reading.Alerts.Any());

        if (billingPeriodId.HasValue)
        {
            query = query.Where(reading => reading.BillingPeriodId == billingPeriodId.Value);
        }

        return await query
            .OrderByDescending(reading => reading.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.Readings.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Reading>> GetByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default)
    {
        return await this.Readings
            .Where(r => r.MeterId == meterId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await this.Readings
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reading>> GetByBillingPeriodIdAsync(Guid billingPeriodId, CancellationToken cancellationToken = default)
    {
        return await this.Readings
            .Where(r => r.BillingPeriodId == billingPeriodId)
            .OrderByDescending(r => r.MeasuredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<MonthlySummarySnapshot> GetMonthlySummarySnapshotAsync(
        Guid billingPeriodId,
        CancellationToken cancellationToken = default)
    {
        var readingsInPeriod = this.Readings.Where(reading => reading.BillingPeriodId == billingPeriodId);

        var readingCount = await readingsInPeriod.CountAsync(cancellationToken);
        var pendingCount = await readingsInPeriod.CountAsync(reading => reading.Status == "Pending", cancellationToken);
        var abnormalCount = await readingsInPeriod.CountAsync(reading => reading.Alerts.Any(), cancellationToken);

        var latestTimestamps = readingsInPeriod
            .GroupBy(reading => new { reading.MeterId, reading.PropertyId, reading.Source })
            .Select(group => new
            {
                group.Key.MeterId,
                group.Key.PropertyId,
                group.Key.Source,
                MeasuredAt = group.Max(reading => reading.MeasuredAt)
            });

        var latestReadings = await (
                from reading in readingsInPeriod
                join latest in latestTimestamps
                    on new { reading.MeterId, reading.PropertyId, reading.Source, reading.MeasuredAt }
                    equals new { latest.MeterId, latest.PropertyId, latest.Source, latest.MeasuredAt }
                select new MonthlySummarySnapshotReading(
                    reading.MeterId,
                    reading.PropertyId,
                    reading.Source,
                    reading.Value,
                    reading.MeasuredAt,
                    reading.Id))
            .ToListAsync(cancellationToken);

        var deduplicatedReadings = latestReadings
            .GroupBy(reading => new
            {
                reading.MeterId,
                reading.PropertyId,
                Source = reading.Source.Trim().ToUpperInvariant()
            })
            .Select(group => group
                .OrderByDescending(reading => reading.MeasuredAt)
                .ThenByDescending(reading => reading.ReadingId)
                .First())
            .ToList();

        return new MonthlySummarySnapshot(
            billingPeriodId,
            readingCount,
            pendingCount,
            abnormalCount,
            deduplicatedReadings);
    }

    public async Task<Reading?> GetLatestByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default)
    {
        return await this.Readings
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
        var query = this.Readings
            .Where(reading => reading.MeterId == meterId && reading.MeasuredAt < measuredAt);

        if (billingPeriodId.HasValue)
        {
            query = query.Where(reading => reading.BillingPeriodId != billingPeriodId);
        }

        return await query
            .OrderByDescending(reading => reading.MeasuredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<decimal> GetHistoricalAverageConsumptionAsync(Guid meterId, CancellationToken cancellationToken = default)
    {
        var consumptions = from reading in this.Readings
                           join previous in this.Readings
                               on reading.PreviousReadingId equals previous.Id
                           where reading.MeterId == meterId
                           let consumption = reading.Value - previous.Value
                           where consumption > 0m
                           select (decimal?)consumption;

        return await consumptions.AverageAsync(cancellationToken) ?? 0m;
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
        return await this.Readings.AnyAsync(r => r.Id == id, cancellationToken);
    }
}
