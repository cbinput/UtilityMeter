namespace CleanMinimalApi.Presentation.Serialization;

using System.Text.Json.Serialization;
using Application.Evidence.Commands.AttachEvidenceToReading;
using Application.Evidence.Commands.UploadEvidence;
using Application.Evidence.Dtos;
using Application.Readings.Commands.CreateReading;
using Application.Readings.Dtos;
using Application.Versions.Entities;
using Endpoints;
using Requests;

/// <summary>
/// JSON serialization context for compile-time source generation.
/// Provides performance benefits and AOT (Ahead-of-Time) compilation support.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(List<Version>))]
[JsonSerializable(typeof(CreateReadingRequest))]
[JsonSerializable(typeof(CreateReadingResponse))]
[JsonSerializable(typeof(ReadingDto))]
[JsonSerializable(typeof(List<ReadingDto>))]
[JsonSerializable(typeof(UploadEvidenceResponse))]
[JsonSerializable(typeof(AttachEvidenceRequest))]
[JsonSerializable(typeof(AttachEvidenceToReadingResponse))]
[JsonSerializable(typeof(EvidenceDto))]
[JsonSerializable(typeof(List<EvidenceDto>))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
