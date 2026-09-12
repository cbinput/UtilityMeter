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

```bash
export ConnectionStrings__UtilityMeterDb='Data Source=/tmp/utilitymeter.db'
export BACKGROUND_JOB_QUEUE_PATH='/tmp/utilitymeter-background-jobs'
```

Start the API:

```bash
dotnet run --project ./src/Presentation
```

Start the Worker in a second terminal:

```bash
dotnet run --project ./src/Worker
```

Example API URLs from the current local launch profile:
- `https://localhost:7032/swagger`
- `http://localhost:5032/swagger`

If the ports differ in your environment, check `src/Presentation/Properties/launchSettings.json`.

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
