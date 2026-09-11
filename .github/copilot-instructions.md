# GitHub Copilot Instructions — Residential Resource Consumption Monitoring System

## Project context

We're building a platform to record, validate, and analyze residential resource consumption (water, electricity, gas, and potentially others) for both individual households and condominiums/communities that have a main meter plus per-house sub-meters.

Original problem: in a condominium, one house's excessive consumption (pool fill, irrigation, a leak), unpaid consumption from some neighbors, and readings the utility company sometimes fails to take correctly, all distort the shared proration. This system replaces the current manual process (photos + a shared spreadsheet) with a structured, auditable record.

The core concept is NOT "store utility bills." It's the measurement as first-class data, with photos, PDFs, and OCR as immutable evidence supporting it.

## Core architectural principle

Never collapse these concepts into a single entity — they must stay independent and traceable:

```
Measurement → Consumption → Billing → Proration → Audit/Analysis
```

Examples this separation must support:
- A resident's reading can be correct even when the company's reading differs.
- A bill can be correct even when the condominium's proration is wrong.
- Proration can be mathematically correct even when one property has abnormal consumption.
- A property can have no reading at all for a given period.

## Domain model

Main hierarchy:

```
Tenant/Organization
 └─ Condominium (optional — an individual household has none)
     └─ Property (house)
         └─ Meter (type: WATER | ELECTRICITY | GAS, extensible via ResourceType)
             └─ Reading (tied to a BillingPeriod)
                 ├─ Evidence (meter photo | utility bill PDF | other document — immutable, in storage, with a hash)
                 ├─ Consumption (calculated: current reading − previous reading)
                 ├─ Validation (outcome of business rules)
                 └─ Alert (if applicable)
```

Key concepts:
- `ResourceType`: extensible enum (Water, Electricity, Gas). Don't hardcode resource-specific logic.
- `BillingPeriod`: states `Open → Collecting → Validating → Closed`. Once `Closed`, it's immutable.
- `Reading`: `value`, `unit`, `measuredAt`, `source` (manual | OCR), `status`.
- `Evidence`: `type` (`MeterPhoto | UtilityBill | Other`), `storageKey`, `hash`. Never the raw file in PostgreSQL.
- `Consumption`: always calculated, never entered manually.
- Condominium-level reconciliation: main meter vs. sum of sub-meters, with a % difference.

## Solution architecture — built on the CleanMinimalApi template (stphnwlsh)

This repo is a fork of `stphnwlsh/CleanMinimalApi` (.NET 10, "Lean Mean Clean Architecture" flavor of Clean Architecture for Minimal APIs). Respect its existing 3-project structure and conventions — don't introduce a different layering scheme:

- **Presentation** — Minimal API endpoints only. Endpoints call into Application via MediatR (`ISender`); no business logic lives here.
- **Application** — owns the domain entities and business logic (this template does not use a separate Domain project). Organize by feature — `Readings`, `Properties`, `Meters`, `BillingPeriods`, `Evidence`, `Reports`, `Alerts` — each folder holding its entity/aggregate, Commands + Handlers, Queries + Handlers, FluentValidation validators, and the repository interface that feature depends on.
- **Infrastructure** — EF Core `DbContext`, repository implementations, entity mappings. Keep everything here `internal`, per the template's existing convention (Infrastructure is referenced by Presentation only for DI wiring, never called directly).

Modular monolith, not microservices — the expected load (tens to hundreds of properties, concentrated in one week per month) doesn't justify that complexity.

### Add a Worker project for async processing

The template ships 3 projects with no background-processing story. Add a 4th project, **Worker** (a separate `BackgroundService` host), for anything that shouldn't block an HTTP request: evidence extraction (OCR), historical/anomaly analysis, monthly report generation, external exports. The Worker dequeues jobs and dispatches the same MediatR commands/queries already defined in Application — keep it thin, don't duplicate business logic there.

Concurrency rule: the API only accepts fast writes (a reading plus its evidence) and enqueues a job. Everything slow runs async in the Worker. Never block an HTTP endpoint on heavy work.

### New Infrastructure abstractions this domain needs

The template doesn't ship object storage or a queue. Add these as interfaces in Application, implemented `internal` in Infrastructure — same pattern as the existing repository interfaces:
- `IObjectStorage` — for evidence files (meter photos, bill PDFs). Implement with MinIO for the self-hosted deployment.
- `IBackgroundJobQueue` — implement with `System.Threading.Channels` for self-hosted; swap for SQS later if this ever moves to AWS.

Start self-hosted (Docker Compose: Presentation + Worker + PostgreSQL + MinIO), no Kubernetes. The interface boundary is what makes a later move to S3/SQS painless.

## Use what the template already provides — don't replace it

- **MediatR** — every write is a Command, every read is a Query, each with its own Handler. Endpoints in Presentation just call `ISender.Send(...)`.
- **FluentValidation** — one validator per Command/Query, wired through the template's existing MediatR pipeline behavior. Use this for structural/input validation (is the value numeric, is the period open, is the required evidence attached).
- **AutoMapper** — map domain entities to Presentation DTOs with profiles; never expose EF entities directly from an endpoint.
- **Serilog** — structured logging (named parameters, never string interpolation).
- **Swashbuckle** — every new endpoint needs proper OpenAPI metadata (summary, response types).
- **Shouldly + NSubstitute** — use these in tests, not FluentAssertions/Moq, to stay consistent with the rest of the solution.

## Business-rule outcomes vs. input validation — don't conflate them

- **Input validation** (FluentValidation) — rejects malformed requests before they reach a handler.
- **Business-rule outcomes** — a structurally valid reading can still be abnormal, inconsistent, or late. These aren't rejected; they're recorded as an `Alert` and surfaced in the dashboard/report. Model this as a domain outcome (e.g. a `Result<Reading>` carrying warnings, or a `Reading` that raises an `Alert` as part of its aggregate) — not as a validation failure.

## Business rules to implement

1. Current reading lower than the previous one → warning/error (unless meter rollover/replacement).
2. Consumption abnormally high vs. that property's historical average → alert with % variation.
3. Missing reading for the period → flag the property as pending.
4. Resident reading vs. company reading mismatch → report the difference; both pieces of evidence stay available.
5. Condominium reconciliation: main meter vs. sum of sub-meters → % difference + list of suspect properties (high consumption, missing reading, bill mismatch).

## Suggested MVP build order

1. Identity and structure: user, organization/condominium, property, meter, resource type.
2. Manual reading: value, date, period, previous reading, calculated consumption.
3. Evidence: photo and PDF upload, linked to the reading, in persistent storage.
4. Validation: the 4 rules above (no OCR yet).
5. Condominium: main meter, sub-meters, monthly reconciliation.
6. Reporting: monthly dashboard, monthly report, missing readings, anomalies, main-vs-submeter difference.

OCR (behind an `IMeterReadingExtractor` interface, swappable implementation), ML, and external APIs come after this works reliably — not before.

## Explicitly out of scope for now

Kubernetes, microservices, complex event streaming, multiple databases, ML, mandatory OCR, Redis (unless a real need appears), complex orchestration.
