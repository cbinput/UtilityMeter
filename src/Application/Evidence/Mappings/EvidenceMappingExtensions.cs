namespace CleanMinimalApi.Application.Evidence.Mappings;

using CleanMinimalApi.Application.Evidence.Dtos;
using CleanMinimalApi.Application.Evidence.Entities;

public static class EvidenceMappingExtensions
{
    public static EvidenceDto ToDto(this Evidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        return new EvidenceDto(
            evidence.Id,
            evidence.FileName,
            evidence.StorageKey,
            evidence.ContentType,
            evidence.Hash,
            evidence.Metadata);
    }

    public static List<EvidenceDto> ToDtoList(this IEnumerable<Evidence> evidences)
    {
        ArgumentNullException.ThrowIfNull(evidences);

        return [.. evidences.Select(e => e.ToDto())];
    }
}
