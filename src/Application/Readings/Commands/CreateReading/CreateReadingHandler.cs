namespace CleanMinimalApi.Application.Readings.Commands.CreateReading;

using CleanMinimalApi.Application.Common.Exceptions;
using CleanMinimalApi.Application.Readings.Entities;
using MediatR;
using Serilog;

public sealed class CreateReadingHandler(IReadingsRepository readingsRepository, ILogger logger)
    : IRequestHandler<CreateReadingCommand, CreateReadingResponse>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CreateReadingResponse> Handle(CreateReadingCommand request, CancellationToken cancellationToken)
    {
        this.logger.Information("Creating reading for meter {MeterId} with value {Value}", request.MeterId, request.Value);

        // Get the previous reading if PreviousReadingId is not provided
        Reading? previousReading;
        if (request.PreviousReadingId.HasValue)
        {
            previousReading = await this.readingsRepository.GetByIdAsync(request.PreviousReadingId.Value, cancellationToken);
            if (previousReading == null)
            {
                throw new NotFoundException($"Previous reading with id {request.PreviousReadingId} not found");
            }
        }
        else
        {
            previousReading = await this.readingsRepository.GetPreviousByMeterAsync(
                request.MeterId,
                request.MeasuredAt,
                request.BillingPeriodId,
                cancellationToken);
        }

        var reading = new Reading
        {
            MeterId = request.MeterId,
            PropertyId = request.PropertyId,
            BillingPeriodId = request.BillingPeriodId,
            PreviousReadingId = previousReading?.Id,
            Value = request.Value,
            Unit = request.Unit,
            MeasuredAt = request.MeasuredAt,
            Source = request.Source,
            Status = "Pending"
        };

        // Check for consumption anomaly
        if (previousReading != null)
        {
            var historicalReadings = await this.readingsRepository.GetByMeterIdAsync(request.MeterId, cancellationToken);
            var historicalReadingsById = historicalReadings.ToDictionary(item => item.Id);
            var historicalConsumptions = historicalReadings
                .Where(item => item.PreviousReadingId.HasValue)
                .Select(item => item.PreviousReadingId.HasValue && historicalReadingsById.TryGetValue(item.PreviousReadingId.Value, out var previous)
                    ? item.Value - previous.Value
                    : (decimal?)null)
                .Where(consumption => consumption.HasValue)
                .Select(consumption => consumption!.Value)
                .Where(consumption => consumption > 0m)
                .ToList();
            var historicalAverage = historicalConsumptions.Count == 0 ? 0m : historicalConsumptions.Average();
            reading.Alerts.AddRange(Reading.GetAlerts(previousReading, reading, historicalAverage));
            if (reading.Alerts.Count > 0)
            {
                reading.Status = "Warning";
                foreach (var alert in reading.Alerts)
                {
                    this.logger.Warning("Reading alert generated: {Message}", alert.Message);
                }
            }
        }

        await this.readingsRepository.AddAsync(reading, cancellationToken);

        this.logger.Information("Reading created with id {ReadingId}", reading.Id);

        return new CreateReadingResponse(reading.Id, reading.Value, reading.Status);
    }
}
