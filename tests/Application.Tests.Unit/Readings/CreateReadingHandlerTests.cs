namespace CleanMinimalApi.Application.Tests.Unit.Readings;

using CleanMinimalApi.Application.Readings;
using CleanMinimalApi.Application.Readings.Commands.CreateReading;
using CleanMinimalApi.Application.Readings.Entities;
using NSubstitute;
using Serilog;
using Shouldly;
using Xunit;

public class CreateReadingHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUseReadingFromPreviousPeriod_WhenCurrentPeriodAlreadyHasAnotherSource()
    {
        var meterId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var previousPeriodId = Guid.NewGuid();
        var currentPeriodId = Guid.NewGuid();
        var measuredAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var previousReading = new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            PropertyId = propertyId,
            BillingPeriodId = previousPeriodId,
            Value = 100m,
            Unit = "m3",
            MeasuredAt = measuredAt.AddMonths(-1),
            Source = "Resident"
        };
        var repository = Substitute.For<IReadingsRepository>();
        var logger = new LoggerConfiguration().CreateLogger();
        Reading? savedReading = null;

        repository.GetPreviousByMeterAsync(meterId, measuredAt, currentPeriodId, Arg.Any<CancellationToken>())
            .Returns(previousReading);
        repository.GetHistoricalAverageConsumptionAsync(meterId, Arg.Any<CancellationToken>())
            .Returns(0m);
        repository.AddAsync(Arg.Any<Reading>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                savedReading = call.Arg<Reading>();
                return Task.CompletedTask;
            });

        var handler = new CreateReadingHandler(repository, logger);

        var response = await handler.Handle(
            new CreateReadingCommand(
                meterId,
                propertyId,
                currentPeriodId,
                null,
                120m,
                "m3",
                measuredAt,
                "Company"),
            CancellationToken.None);

        response.Status.ShouldBe("Pending");
        savedReading.ShouldNotBeNull();
        savedReading.PreviousReadingId.ShouldBe(previousReading.Id);
        await repository.Received(1).GetHistoricalAverageConsumptionAsync(meterId, Arg.Any<CancellationToken>());
        await repository.DidNotReceive().GetLatestByMeterIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
