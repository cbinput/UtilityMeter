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
            // Get the latest reading for this meter
            previousReading = await this.readingsRepository.GetLatestByMeterIdAsync(request.MeterId, cancellationToken);
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
            var alert = Reading.GetAlert(previousReading, reading);
            if (alert != null)
            {
                reading.Status = "Warning";
                this.logger.Warning("Reading alert generated: {Message}", alert.Message);
            }
        }

        await this.readingsRepository.AddAsync(reading, cancellationToken);

        this.logger.Information("Reading created with id {ReadingId}", reading.Id);

        return new CreateReadingResponse(reading.Id, reading.Value, reading.Status);
    }
}
