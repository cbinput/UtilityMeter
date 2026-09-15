# UtilityMeter Overview

## What UtilityMeter is

UtilityMeter is a residential resource consumption monitoring system for individual households and condominiums. It is designed to make meter readings traceable, attach evidence to each reading, and support later validation, reporting, and reconciliation.

## What problem it solves

In shared-consumption environments, a single abnormal property, a missed utility-company reading, or an unpaid share can distort manual proration. UtilityMeter creates a structured record so the team can:

- keep resident readings and company readings side by side
- preserve evidence such as photos and PDFs
- calculate consumption from meter history
- detect anomalies and pending readings
- reconcile condominium main meters against property sub-meters

## Core concepts

UtilityMeter keeps these concepts separate so the audit trail stays clear:

1. **Measurement**: the captured meter reading
2. **Consumption**: the delta between readings
3. **Billing**: utility-company billing information
4. **Proration**: condominium or community cost distribution
5. **Audit and analysis**: mismatch detection, anomalies, and reports

## Current capabilities

- Create and query readings by id, meter, property, and billing period
- Upload evidence files and attach them to readings
- Detect warnings for lower-than-previous readings
- Detect abnormally high consumption against historical average
- List pending readings and abnormal readings
- Generate monthly summaries and condominium reconciliation views
- Queue heavier operations for background processing

## Main API groups

- `/api/readings`
- `/api/evidence`
- `/api/reports`

Swagger is available when the API is running.

## Local setup

### Docker Compose

Recommended for the full experience.

```bash
docker compose up --build
```

Available services:
- API / Swagger: `http://localhost:8080/swagger`
- MinIO API: `http://localhost:9000`
- MinIO Console: `http://localhost:9001`

Stop the stack:

```bash
docker compose down
```

### .NET CLI

For local development without containers, run API and Worker separately and share the same database and queue path.

macOS/Linux:

```bash
export ConnectionStrings__UtilityMeterDb='Data Source=/tmp/utilitymeter.db'
export BACKGROUND_JOB_QUEUE_PATH='/tmp/utilitymeter-background-jobs'
```

Windows PowerShell:

```powershell
$env:ConnectionStrings__UtilityMeterDb = "Data Source=$env:TEMP\\utilitymeter.db"
$env:BACKGROUND_JOB_QUEUE_PATH = "$env:TEMP\\utilitymeter-background-jobs"
```

For evidence upload flows, either use Docker Compose or also configure object storage for both processes, for example with MinIO:

macOS/Linux:

```bash
export ObjectStorage__Provider='Minio'
export ObjectStorage__Minio__Endpoint='localhost:9000'
export ObjectStorage__Minio__AccessKey='utilitymeter'
export ObjectStorage__Minio__SecretKey='utilitymeter'
export ObjectStorage__Minio__BucketName='utilitymeter-evidence'
export ObjectStorage__Minio__UseSsl='false'
```

Windows PowerShell:

```powershell
$env:ObjectStorage__Provider = 'Minio'
$env:ObjectStorage__Minio__Endpoint = 'localhost:9000'
$env:ObjectStorage__Minio__AccessKey = 'utilitymeter'
$env:ObjectStorage__Minio__SecretKey = 'utilitymeter'
$env:ObjectStorage__Minio__BucketName = 'utilitymeter-evidence'
$env:ObjectStorage__Minio__UseSsl = 'false'
```

The application verifies the configured MinIO bucket during startup and creates it automatically if it does not already exist.

Start the API:

```bash
dotnet run --project ./src/Presentation
```

Start the Worker in a second terminal:

```bash
dotnet run --project ./src/Worker
```

Use the URLs printed by `dotnet run`, or check `src/Presentation/Properties/launchSettings.json` for the current local profile values.

## Project layout

```text
src/
  Application/      business rules, commands, queries, validators
  Infrastructure/   persistence, storage, queue implementations
  Presentation/     minimal API endpoints and OpenAPI setup
  Worker/           background job host
tests/              unit and integration tests
docs/               bilingual product and architecture docs
```

## More documentation

- [Architecture](./architecture.md)
