namespace CleanMinimalApi.Presentation.Endpoints;

using CleanMinimalApi.Application.Common.Exceptions;
using CleanMinimalApi.Application.Readings.Commands.CreateReading;
using CleanMinimalApi.Application.Readings.Dtos;
using CleanMinimalApi.Application.Readings.Mappings;
using CleanMinimalApi.Application.Readings.Queries.GetReadingById;
using CleanMinimalApi.Application.Readings.Queries.GetReadingsByBillingPeriod;
using CleanMinimalApi.Application.Readings.Queries.GetReadingsByMeter;
using CleanMinimalApi.Application.Readings.Queries.GetReadingsByProperty;
using CleanMinimalApi.Presentation.Filters;
using CleanMinimalApi.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class ReadingEndpoints
{
    public static WebApplication MapReadingEndpoints(this WebApplication app)
    {
        var root = app.MapGroup("/api/readings")
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory)
            .WithTags("readings")
            .WithDescription("Create and retrieve utility meter readings");

        root.MapPost("/", CreateReading)
            .Produces<CreateReadingResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new meter reading")
            .WithDescription("\n    POST /readings");

        root.MapGet("/{id}", GetReadingById)
            .Produces<ReadingDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get a reading by its Id")
            .WithDescription("\n    GET /readings/00000000-0000-0000-0000-000000000000");

        root.MapGet("/meter/{meterId}", GetReadingsByMeter)
            .Produces<List<ReadingDto>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get all readings for a meter")
            .WithDescription("\n    GET /readings/meter/00000000-0000-0000-0000-000000000000");

        root.MapGet("/property/{propertyId}", GetReadingsByProperty)
            .Produces<List<ReadingDto>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get all readings for a property")
            .WithDescription("\n    GET /readings/property/00000000-0000-0000-0000-000000000000");

        root.MapGet("/billing-period/{billingPeriodId}", GetReadingsByBillingPeriod)
            .Produces<List<ReadingDto>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get all readings for a billing period")
            .WithDescription("\n    GET /readings/billing-period/00000000-0000-0000-0000-000000000000");

        return app;
    }

    public static async Task<Created<CreateReadingResponse>> CreateReading(
        [Validate][FromBody] CreateReadingRequest request,
        [FromServices] ISender sender)
    {
        var command = new CreateReadingCommand(
            request.MeterId,
            request.PropertyId,
            request.BillingPeriodId,
            request.PreviousReadingId,
            request.Value,
            request.Unit,
            request.MeasuredAt,
            request.Source);

        var result = await sender.Send(command);
        return TypedResults.Created($"/api/readings/{result.Id}", result);
    }

    public static async Task<Ok<ReadingDto>> GetReadingById(
        [Validate][FromRoute] Guid id,
        [FromServices] ISender sender)
    {
        var query = new GetReadingByIdQuery(id);
        var reading = await sender.Send(query);
        return TypedResults.Ok(reading.ToDto());
    }

    public static async Task<Ok<List<ReadingDto>>> GetReadingsByMeter(
        [Validate][FromRoute] Guid meterId,
        [FromServices] ISender sender)
    {
        var query = new GetReadingsByMeterQuery(meterId);
        var readings = await sender.Send(query);
        return TypedResults.Ok(readings.ToDtoList());
    }

    public static async Task<Ok<List<ReadingDto>>> GetReadingsByProperty(
        [Validate][FromRoute] Guid propertyId,
        [FromServices] ISender sender)
    {
        var query = new GetReadingsByPropertyQuery(propertyId);
        var readings = await sender.Send(query);
        return TypedResults.Ok(readings.ToDtoList());
    }

    public static async Task<Ok<List<ReadingDto>>> GetReadingsByBillingPeriod(
        [Validate][FromRoute] Guid billingPeriodId,
        [FromServices] ISender sender)
    {
        var query = new GetReadingsByBillingPeriodQuery(billingPeriodId);
        var readings = await sender.Send(query);
        return TypedResults.Ok(readings.ToDtoList());
    }
}
