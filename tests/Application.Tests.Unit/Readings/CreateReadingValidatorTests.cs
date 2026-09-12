namespace CleanMinimalApi.Application.Tests.Unit.Readings;

using System;
using CleanMinimalApi.Application.Readings.Commands.CreateReading;
using Shouldly;
using Xunit;

public class CreateReadingValidatorTests
{
    [Fact]
    public void Validate_ShouldRejectMissingRequiredFields()
    {
        var result = new CreateReadingValidator().Validate(new CreateReadingCommand(
            Guid.Empty,
            null,
            null,
            null,
            -1m,
            string.Empty,
            default,
            string.Empty));

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
}