namespace CleanMinimalApi.Application.Tests.Unit.Readings;

using System;
using CleanMinimalApi.Application.Alerts;
using CleanMinimalApi.Application.Readings.Entities;
using Shouldly;
using Xunit;

public class ReadingConsumptionTests
{
    [Fact]
    public void CalculateConsumption_ShouldReturnPositiveDelta_WhenCurrentExceedsPrevious()
    {
        // Arrange
        var previous = new Reading
        {
            Value = 100m,
            MeasuredAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        var current = new Reading
        {
            Value = 125m,
            MeasuredAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };

        // Act
        var consumption = Reading.CalculateConsumption(previous, current);

        // Assert
        consumption.ShouldBe(25m);
    }

    [Fact]
    public void GetAlert_ShouldFlagCurrentReadingLowerThanPreviousReading()
    {
        // Arrange
        var previous = new Reading
        {
            Value = 120m,
            MeasuredAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        var current = new Reading
        {
            Value = 90m,
            MeasuredAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)
        };

        // Act
        var alert = Reading.GetAlert(previous, current);

        // Assert
        alert.ShouldNotBeNull();
        alert.Severity.ShouldBe(AlertSeverity.Warning);
        alert.Message.ShouldContain("lower than the previous reading");
    }
}
