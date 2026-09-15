namespace CleanMinimalApi.Infrastructure.Tests.Integration.Databases.UtilityMeter;

using System;
using System.Threading.Tasks;
using CleanMinimalApi.Application.Alerts;
using CleanMinimalApi.Application.Readings.Entities;
using CleanMinimalApi.Infrastructure.Databases.UtilityMeter;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

public class EntityFrameworkReadingsRepositoryTests
{
    private static UtilityMeterDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<UtilityMeterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new UtilityMeterDbContext(options);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldSaveAndRetrieveReading()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new EntityFrameworkReadingsRepository(context);

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            BillingPeriodId = Guid.NewGuid(),
            Value = 123.45m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow,
            Source = "Manual",
            Status = "Recorded",
            EvidenceIds = [Guid.NewGuid()]
        };

        // Act
        await repository.AddAsync(reading);
        var retrieved = await repository.GetByIdAsync(reading.Id);

        // Assert
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(reading.Id);
        retrieved.Value.ShouldBe(123.45m);
        retrieved.Unit.ShouldBe("m3");
        retrieved.EvidenceIds.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenReadingExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new EntityFrameworkReadingsRepository(context);

        var reading = new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = Guid.NewGuid(),
            Value = 50.0m,
            Unit = "kWh",
            MeasuredAt = DateTimeOffset.UtcNow,
            Source = "Manual",
            Status = "Recorded"
        };

        await repository.AddAsync(reading);

        // Act
        var exists = await repository.ExistsAsync(reading.Id);
        var notExists = await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.ShouldBeTrue();
        notExists.ShouldBeFalse();
    }

    [Fact]
    public async Task GetHistoricalAverageConsumptionAsync_ShouldAveragePositiveConsumptions()
    {
        using var context = CreateDbContext();
        var repository = new EntityFrameworkReadingsRepository(context);
        var meterId = Guid.NewGuid();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();

        await repository.AddAsync(new Reading
        {
            Id = firstId,
            MeterId = meterId,
            Value = 100m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow.AddMonths(-2),
            Source = "Resident",
            Status = "Recorded"
        });
        await repository.AddAsync(new Reading
        {
            Id = secondId,
            MeterId = meterId,
            PreviousReadingId = firstId,
            Value = 130m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow.AddMonths(-1),
            Source = "Resident",
            Status = "Recorded"
        });
        await repository.AddAsync(new Reading
        {
            Id = thirdId,
            MeterId = meterId,
            PreviousReadingId = secondId,
            Value = 190m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow,
            Source = "Resident",
            Status = "Recorded"
        });

        var average = await repository.GetHistoricalAverageConsumptionAsync(meterId);

        average.ShouldBe(45m);
    }

    [Fact]
    public async Task GetMonthlySummarySnapshotAsync_ShouldReturnCountsAndLatestReadingsPerSource()
    {
        using var context = CreateDbContext();
        var repository = new EntityFrameworkReadingsRepository(context);
        var billingPeriodId = Guid.NewGuid();
        var meterId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        await repository.AddAsync(new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            PropertyId = propertyId,
            BillingPeriodId = billingPeriodId,
            Value = 100m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow.AddDays(-2),
            Source = "Resident",
            Status = "Pending"
        });
        await repository.AddAsync(new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            PropertyId = propertyId,
            BillingPeriodId = billingPeriodId,
            Value = 105m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow.AddDays(-1),
            Source = "Resident",
            Status = "Warning",
            Alerts = { new Alert { Message = "abnormal", Severity = AlertSeverity.Warning, Type = AlertType.AbnormallyHighConsumption } }
        });
        await repository.AddAsync(new Reading
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            PropertyId = propertyId,
            BillingPeriodId = billingPeriodId,
            Value = 110m,
            Unit = "m3",
            MeasuredAt = DateTimeOffset.UtcNow,
            Source = "Company",
            Status = "Recorded"
        });

        var snapshot = await repository.GetMonthlySummarySnapshotAsync(billingPeriodId);

        snapshot.ReadingCount.ShouldBe(3);
        snapshot.PendingCount.ShouldBe(1);
        snapshot.AbnormalCount.ShouldBe(1);
        snapshot.LatestReadingsBySource.Count.ShouldBe(2);
        snapshot.LatestReadingsBySource.ShouldContain(item => item.Source == "Resident" && item.Value == 105m);
        snapshot.LatestReadingsBySource.ShouldContain(item => item.Source == "Company" && item.Value == 110m);
    }
}
