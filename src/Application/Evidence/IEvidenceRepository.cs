namespace CleanMinimalApi.Application.Evidence;

using CleanMinimalApi.Application.Evidence.Entities;

public interface IEvidenceRepository
{
    public Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<List<Evidence>> GetByReadingIdAsync(Guid readingId, CancellationToken cancellationToken = default);
    public Task AddAsync(Evidence evidence, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
