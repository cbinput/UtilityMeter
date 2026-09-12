namespace CleanMinimalApi.Infrastructure.Tests.Integration.Databases.UtilityMeter;

using System;
using System.Threading.Tasks;
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
}
