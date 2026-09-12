namespace CleanMinimalApi.Application.Readings;

using CleanMinimalApi.Application.Readings.Entities;

public interface IReadingsRepository
{
    public Task<Reading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
    public Task<List<Reading>> GetByBillingPeriodIdAsync(Guid billingPeriodId, CancellationToken cancellationToken = default);
    public Task<Reading?> GetLatestByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default);
    public Task AddAsync(Reading reading, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Reading reading, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
