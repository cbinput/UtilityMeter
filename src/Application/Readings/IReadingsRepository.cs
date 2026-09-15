namespace CleanMinimalApi.Application.Readings;

using CleanMinimalApi.Application.Readings.Entities;

public interface IReadingsRepository
{
    public Task<List<Reading>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetPendingAsync(Guid? billingPeriodId = null, Guid? propertyId = null, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetAbnormalAsync(Guid? billingPeriodId = null, CancellationToken cancellationToken = default);
    public Task<Reading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByBillingPeriodIdAsync(Guid billingPeriodId, CancellationToken cancellationToken = default);
    public Task<MonthlySummarySnapshot> GetMonthlySummarySnapshotAsync(Guid billingPeriodId, CancellationToken cancellationToken = default);
    public Task<Reading?> GetLatestByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default);
    public Task<Reading?> GetPreviousByMeterAsync(Guid meterId, DateTimeOffset measuredAt, Guid? billingPeriodId, CancellationToken cancellationToken = default);
    public Task<decimal> GetHistoricalAverageConsumptionAsync(Guid meterId, CancellationToken cancellationToken = default);
    public Task AddAsync(Reading reading, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Reading reading, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record MonthlySummarySnapshot(
    Guid BillingPeriodId,
    int ReadingCount,
    int PendingCount,
    int AbnormalCount,
    IReadOnlyList<MonthlySummarySnapshotReading> LatestReadingsBySource);

public sealed record MonthlySummarySnapshotReading(
    Guid MeterId,
    Guid? PropertyId,
    string Source,
    decimal Value,
    DateTimeOffset MeasuredAt,
    Guid ReadingId);
