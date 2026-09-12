namespace CleanMinimalApi.Application.Tests.Unit.Common;

using System;
using System.Collections.Generic;
using CleanMinimalApi.Application.BillingPeriods.Entities;
using CleanMinimalApi.Application.Common;
using CleanMinimalApi.Application.Common.Enums;
using CleanMinimalApi.Application.Evidence.Entities;
using Shouldly;
using Xunit;

public class DomainModelTests
{
    [Fact]
    public void BillingPeriod_ShouldBeImmutable_WhenClosed()
    {
        // Arrange
        var period = new BillingPeriod
        {
            Id = Guid.NewGuid(),
            Start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero),
            Status = BillingPeriodStatus.Closed
        };

        // Act
        var isImmutable = period.IsImmutable;

        // Assert
        isImmutable.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => period.SetStatus(BillingPeriodStatus.Open));
    }

    [Fact]
    public void Evidence_ShouldContainImmutableMetadata_WithoutRawFileStorage()
    {
        // Arrange
        var evidence = new Evidence
        {
            Id = Guid.NewGuid(),
            StorageKey = "evidence/reading-123/meter-photo.jpg",
            FileName = "meter-photo.jpg",
            ContentType = "image/jpeg",
            Hash = "sha256:abc123",
            Metadata = new Dictionary<string, string>
            {
                ["meterId"] = "meter-42",
                ["source"] = "manual"
            }
        };

        // Assert
        evidence.StorageKey.ShouldNotBeNullOrWhiteSpace();
        evidence.Hash.ShouldNotBeNullOrWhiteSpace();
        evidence.RawContent.ShouldBeNull();
        Evidence.IsImmutable.ShouldBeTrue();
    }

    [Fact]
    public void ValidationResult_ShouldSeparateBusinessWarnings_FromStructuralFailures()
    {
        // Arrange
        var warning = ValidationResult.Warning("Consumption is unusually high for the property.", "meter-42");
        var failure = ValidationResult.Failure("Value is required for the reading.", "meter-42");

        // Assert
        warning.IsBusinessRule.ShouldBeTrue();
        warning.IsValid.ShouldBeTrue();

        failure.IsBusinessRule.ShouldBeFalse();
        failure.IsValid.ShouldBeFalse();
    }
}
