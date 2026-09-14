---
name: feature-slice
description: >-
  Add or modify a feature in this .NET 10 CleanMinimalApi-based solution. Use
  when a task touches endpoints, MediatR commands or queries, validation,
  mappings, repositories, or tests across the Presentation, Application,
  Infrastructure, and Worker projects.
---

# Skill: Feature Slice

Use this skill when implementing or changing product behavior in UtilityMeter.

## Repository rules

- Keep the existing four-project structure: `src/Presentation`, `src/Application`, `src/Infrastructure`, `src/Worker`.
- Keep **Presentation** thin. Endpoints should map HTTP requests to `ISender.Send(...)`, attach validation filters, and expose OpenAPI metadata.
- Put business logic in **Application** feature folders such as `Readings`, `Evidence`, `Reports`, `Alerts`, `Meters`, `Properties`, and `BillingPeriods`.
- Keep **Infrastructure** as implementation detail only. Repository and service implementations stay `internal`.
- Use **Worker** only for background execution of existing Application commands and queries.

## Expected implementation pattern

1. Start from the feature folder in `src/Application/<FeatureName>/`.
2. Add or update:
   - entities or aggregates
   - commands and handlers for writes
   - queries and handlers for reads
   - FluentValidation validators
   - mapping profiles and DTOs
   - the repository interface required by that feature
3. Wire persistence or external-service details in `src/Infrastructure`.
4. Expose or update endpoints in `src/Presentation/Endpoints`.
5. Add or update tests in `tests/`.

## Project conventions

- Use MediatR for every command and query.
- Use FluentValidation for malformed input, not for domain warnings.
- Use AutoMapper-backed DTO mapping patterns already present in the feature folders.
- Use Serilog structured logging when logging is necessary.
- Use Shouldly and NSubstitute in tests.
- Do not expose EF Core entities directly from endpoints.

## Files worth checking first

- `/src/Application/DependencyInjection.cs`
- `/src/Presentation/Endpoints/ReadingEndpoints.cs`
- `/src/Presentation/Endpoints/EvidenceEndpoints.cs`
- `/src/Presentation/Endpoints/ReportEndpoints.cs`
- `/src/Worker/QueuedBackgroundWorker.cs`
- `/docs/en/architecture.md`
