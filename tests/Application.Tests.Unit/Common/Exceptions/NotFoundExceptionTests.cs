namespace CleanMinimalApi.Application.Tests.Unit.Common.Exceptions;

using CleanMinimalApi.Application.Common.Enums;
using CleanMinimalApi.Application.Common.Exceptions;
using Shouldly;
using Xunit;

public class NotFoundExceptionTests
{
    [Fact]
    public void ThrowIfNull_ShouldNotThrow_NotFoundException()
    {
        // Arrange
        var entityType = EntityType.Reading;
        var argument = new object();

        // Act
        var result = Should.NotThrow(() =>
        {
            NotFoundException.ThrowIfNull(argument, entityType);

            return true;
        });

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ThrowIfNull_ShouldThrow_NotFoundException()
    {
        // Arrange
        var entityType = EntityType.Reading;
        object? argument = null;

        // Act
        var result = Should.Throw<NotFoundException>(() =>
        {
            NotFoundException.ThrowIfNull(argument, entityType);

            return true;
        });

        // Assert
        _ = result.ShouldNotBeNull();

        result.Message.ShouldBe("The Reading with the supplied id was not found.");
    }

    [Fact]
    public void Throw_ShouldThrow_NotFoundException()
    {
        // Arrange
        var entityType = EntityType.Reading;

        // Act
        var result = Should.Throw<NotFoundException>(() =>
        {
            NotFoundException.Throw(entityType);

            return true;
        });

        // Assert
        _ = result.ShouldNotBeNull();

        result.Message.ShouldBe("The Reading with the supplied id was not found.");
    }
}
