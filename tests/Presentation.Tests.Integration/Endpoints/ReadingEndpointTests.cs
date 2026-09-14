namespace CleanMinimalApi.Presentation.Tests.Integration.Endpoints;

using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

public class ReadingEndpointTests : IDisposable
{
    private CleanMinimalApiApplication application = new();

    [Fact]
    public async Task CreateReading_ShouldReturnBadRequest_WhenCommandValidationFails()
    {
        using var client = this.application.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/readings", new
        {
            meterId = Guid.Empty,
            propertyId = Guid.NewGuid(),
            billingPeriodId = Guid.NewGuid(),
            previousReadingId = (Guid?)null,
            value = -1m,
            unit = "",
            measuredAt = default(DateTimeOffset),
            source = ""
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.application.Dispose();
            this.application = null!;
        }
    }
}
