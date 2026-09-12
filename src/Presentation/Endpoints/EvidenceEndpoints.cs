namespace CleanMinimalApi.Presentation.Endpoints;

using CleanMinimalApi.Application.Common.Exceptions;
using CleanMinimalApi.Application.Evidence.Commands.AttachEvidenceToReading;
using CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;
using CleanMinimalApi.Application.Evidence.Dtos;
using CleanMinimalApi.Application.Evidence.Mappings;
using CleanMinimalApi.Application.Evidence.Queries.GetEvidenceByReading;
using CleanMinimalApi.Presentation.Filters;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class EvidenceEndpoints
{
    public static WebApplication MapEvidenceEndpoints(this WebApplication app)
    {
        var root = app.MapGroup("/api/evidence")
            .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory)
            .WithTags("evidence")
            .WithDescription("Upload and manage evidence for meter readings");

        _ = root.MapPost("/upload", UploadEvidence)
            .Produces<UploadEvidenceResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Upload evidence file (photo or PDF)")
            .WithDescription("\n    POST /evidence/upload")
            .Accepts<IFormFile>("multipart/form-data");

        _ = root.MapPost("/attach", AttachEvidenceToReading)
            .Produces<AttachEvidenceToReadingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Attach evidence to a reading")
            .WithDescription("\n    POST /evidence/attach");

        _ = root.MapGet("/reading/{readingId}", GetEvidenceByReading)
            .Produces<List<EvidenceDto>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get all evidence for a reading")
            .WithDescription("\n    GET /evidence/reading/00000000-0000-0000-0000-000000000000");

        return app;
    }

    public static async Task<Results<Created<UploadEvidenceResponse>, ValidationProblem, ProblemHttpResult>> UploadEvidence(
        IFormFile file,
        [FromServices] ISender sender)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return TypedResults.Problem("No file provided", "File is required", StatusCodes.Status400BadRequest);
            }

            using var stream = file.OpenReadStream();
            var command = new UploadEvidenceCommand(file.FileName, file.ContentType, stream);
            var result = await sender.Send(command);

            return TypedResults.Created($"/api/evidence/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.StackTrace, ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<Results<Ok<AttachEvidenceToReadingResponse>, ValidationProblem, NotFound<string>, ProblemHttpResult>> AttachEvidenceToReading(
        [Validate][FromBody] AttachEvidenceRequest request,
        [FromServices] ISender sender)
    {
        try
        {
            var command = new AttachEvidenceToReadingCommand(request.ReadingId, request.EvidenceId);
            var result = await sender.Send(command);
            return TypedResults.Ok(result);
        }
        catch (NotFoundException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.StackTrace, ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<Results<Ok<List<EvidenceDto>>, ProblemHttpResult>> GetEvidenceByReading(
        [Validate][FromRoute] Guid readingId,
        [FromServices] ISender sender)
    {
        try
        {
            var query = new GetEvidenceByReadingQuery(readingId);
            var evidences = await sender.Send(query);
            return TypedResults.Ok(evidences.ToDtoList());
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.StackTrace, ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}

public sealed record AttachEvidenceRequest(Guid ReadingId, Guid EvidenceId);
