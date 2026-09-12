namespace CleanMinimalApi.Application.Readings.Mappings;

using CleanMinimalApi.Application.Readings.Dtos;
using CleanMinimalApi.Application.Readings.Entities;

public static class ReadingMappingExtensions
{
    public static ReadingDto ToDto(this Reading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        return new ReadingDto(
            reading.Id,
            reading.MeterId,
            reading.PropertyId,
            reading.BillingPeriodId,
            reading.PreviousReadingId,
            reading.Value,
            reading.Unit,
            reading.MeasuredAt,
            reading.Source,
            reading.Status,
            reading.EvidenceIds,
            reading.Alerts);
    }

    public static List<ReadingDto> ToDtoList(this IEnumerable<Reading> readings)
    {
        ArgumentNullException.ThrowIfNull(readings);

        return [.. readings.Select(r => r.ToDto())];
    }
}
