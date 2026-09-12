# UtilityMeter

UtilityMeter is a residential utility metering platform for water, electricity, gas, and other resource types. It helps individual households and condominiums record readings, keep evidence, detect anomalies, and compare resident readings with company readings without mixing measurement, consumption, billing, and proration into a single concept.

## Language

- English docs: [/docs/en/readme.md](./docs/en/readme.md)
- Documentación en español: [/docs/es/readme.md](./docs/es/readme.md)

## What it solves

- Replaces ad-hoc spreadsheets, chat messages, and photo folders with traceable meter records.
- Keeps meter readings and evidence auditable, even when the utility bill and the resident reading do not match.
- Helps condominiums reconcile a main meter against sub-meters and identify suspect properties.
- Supports asynchronous processing for heavier tasks such as OCR, anomaly analysis, exports, and report generation.

## What the software does

- Registers meter readings per property, meter, and billing period.
- Calculates consumption from the previous reading.
- Stores immutable evidence references for photos and PDFs.
- Flags suspicious situations such as decreased readings or abnormally high consumption.
- Exposes reporting endpoints for pending readings, abnormal readings, monthly summaries, and condominium reconciliation.

## Architecture at a glance

- **Presentation**: Minimal API endpoints and OpenAPI/Swagger.
- **Application**: Domain rules, commands, queries, validation, and MediatR handlers.
- **Infrastructure**: EF Core persistence, object storage, and background job queue implementations.
- **Worker**: Background host that processes queued jobs.

More detail and diagrams:
- [/docs/en/architecture.md](./docs/en/architecture.md)
- [/docs/es/architecture.md](./docs/es/architecture.md)

## Run locally

### Recommended: Docker Compose

This is the fastest way to run the full stack with the API, Worker, shared persistence, and MinIO object storage.

```bash
docker compose up --build
```

Useful URLs:
- API / Swagger: http://localhost:8080/swagger
- MinIO API: http://localhost:9000
- MinIO Console: http://localhost:9001

To stop everything:

```bash
docker compose down
```

### Manual: .NET CLI (macOS/Linux example)

Use this when you want to run the API and Worker directly from the SDK. The commands below are shell examples for macOS/Linux.

1. Export shared environment variables:

```bash
export ConnectionStrings__UtilityMeterDb='Data Source=/tmp/utilitymeter.db'
export BACKGROUND_JOB_QUEUE_PATH='/tmp/utilitymeter-background-jobs'
```

2. Start the API:

```bash
dotnet run --project ./src/Presentation
```

3. In another terminal, start the Worker:

```bash
dotnet run --project ./src/Worker
```

Use the URLs printed by `dotnet run`, or check `src/Presentation/Properties/launchSettings.json` for the current local profile values.

## Repository structure

```text
src/
  Application/
  Infrastructure/
  Presentation/
  Worker/
tests/
docs/
  en/
  es/
```

## Current documentation

- English overview: [/docs/en/readme.md](./docs/en/readme.md)
- English architecture: [/docs/en/architecture.md](./docs/en/architecture.md)
- Resumen en español: [/docs/es/readme.md](./docs/es/readme.md)
- Arquitectura en español: [/docs/es/architecture.md](./docs/es/architecture.md)
