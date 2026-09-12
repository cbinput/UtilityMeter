namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter;

using Application.Evidence;
using Application.Evidence.Entities;
using Microsoft.EntityFrameworkCore;

internal class EntityFrameworkEvidenceRepository(UtilityMeterDbContext context) : IEvidenceRepository
{
    private readonly UtilityMeterDbContext context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Evidence.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Evidence>> GetByReadingIdAsync(Guid readingId, CancellationToken cancellationToken = default)
    {
        // Get the reading to get its EvidenceIds
        var reading = await this.context.Readings.FirstOrDefaultAsync(r => r.Id == readingId, cancellationToken);
        if (reading == null)
        {
            return [];
        }

        // Get all evidence for the reading
        return await this.context.Evidence
            .Where(e => reading.EvidenceIds.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        _ = await this.context.Evidence.AddAsync(evidence, cancellationToken);
        _ = await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Evidence.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
