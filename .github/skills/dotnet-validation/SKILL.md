---
name: dotnet-validation
description: >-
  Validate changes in this .NET 10 repository. Use when a task requires build,
  test, Docker, or local-run verification, especially for Presentation, Worker,
  queue, storage, or repository-wide changes.
---

# Skill: .NET Validation

Use the repository's existing validation paths before finishing changes.

## Default validation path

- Restore/build/test from the solution root: `/CleanMinimalApi.sln`
- Prefer the repository's existing commands and workflow shape instead of inventing new scripts or tooling.

## Commands to prefer

### Fast local validation

```bash
dotnet build /home/runner/work/UtilityMeter/UtilityMeter/CleanMinimalApi.sln
dotnet test /home/runner/work/UtilityMeter/UtilityMeter/CleanMinimalApi.sln
```

### Container-aligned validation

```bash
docker build /home/runner/work/UtilityMeter/UtilityMeter --target coverage --output type=local,dest=out
```

## When background flows are involved

- Run both Presentation and Worker, or use Docker Compose, before trusting end-to-end queue behavior.
- When evidence upload or MinIO behavior changes, validate with shared object-storage configuration instead of API-only runs.

## Testing conventions

- Use existing xUnit projects under `/tests`.
- Keep test style aligned with Shouldly and NSubstitute.
- Do not add new validation frameworks unless the repository already uses them.

## Workflow alignment

- The CI workflow already covers CodeQL plus Docker-based build/test/publish behavior.
- Prefer changes that stay compatible with `.github/workflows/build-pipeline.yml` and `Dockerfile`.
