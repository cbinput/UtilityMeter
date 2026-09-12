# UtilityMeter Architecture

## Overview

UtilityMeter follows the repository's four-project structure:

- **Presentation** receives HTTP requests and exposes Swagger/OpenAPI.
- **Application** contains domain logic, MediatR commands and queries, validators, and DTO mappings.
- **Infrastructure** provides SQLite/EF Core persistence, object storage, and background job queue implementations.
- **Worker** consumes queued jobs and dispatches them back into Application handlers.

## High-level architecture

```mermaid
flowchart LR
    Client[User or integration] --> API[Presentation API]
    API --> App[Application]
    App --> Infra[Infrastructure]
    Infra --> Db[(SQLite / EF Core)]
    Infra --> Storage[(MinIO or in-memory object storage)]
    API --> Queue[IBackgroundJobQueue]
    Queue --> Worker[Worker service]
    Worker --> App
```

## Domain flow

```mermaid
flowchart LR
    Measurement[Measurement / Reading] --> Consumption[Consumption calculation]
    Consumption --> Billing[Billing comparison]
    Billing --> Proration[Proration / reconciliation]
    Proration --> Audit[Audit and analysis]
    Evidence[Evidence files] --> Measurement
```

## Main runtime components

### API

The API exposes grouped endpoints for:
- readings
- evidence
- reports

It stays thin: endpoints forward commands and queries to `ISender`.

### Application

The Application project owns:
- readings
- evidence
- reports
- alerts
- background job contracts
- storage abstractions

Business outcomes such as alerts are recorded in the reading/report flow instead of being treated as malformed input.

### Infrastructure

Infrastructure currently provides:
- EF Core data access
- SQLite when a connection string is configured
- in-memory database fallback when no connection string is present
- MinIO-backed object storage when configured
- channel-based background job dispatch with JSON persistence under `BACKGROUND_JOB_QUEUE_PATH`

### Worker

The Worker is a separate host that dequeues jobs for:
- OCR extraction
- anomaly analysis
- report generation
- export processing

## Sequence: create reading with evidence and async follow-up

```mermaid
sequenceDiagram
    actor User
    participant API as Presentation API
    participant App as Application
    participant Repo as Readings Repository
    participant Storage as Object Storage
    participant Queue as Background Job Queue
    participant Worker as Worker

    User->>API: POST /api/evidence/upload
    API->>App: UploadEvidenceCommand
    App->>Storage: Save file
    App-->>API: Evidence id

    User->>API: POST /api/readings
    API->>App: CreateReadingCommand
    App->>Repo: Load previous reading
    App->>Repo: Save reading + alerts
    App-->>API: Reading created

    User->>API: POST /api/evidence/attach
    API->>App: AttachEvidenceToReadingCommand
    App->>Repo: Link evidence to reading
    App-->>API: Attachment confirmed

    User->>API: POST /api/reports/abnormal-readings/{billingPeriodId}/analyze
    API->>Queue: Enqueue anomaly analysis job
    API-->>User: 202 Accepted
    Worker->>Queue: Dequeue job
    Worker->>App: ProcessAnomalyAnalysisJobCommand
    App-->>Worker: Analysis completed
```

## Sequence: condominium reconciliation

```mermaid
sequenceDiagram
    actor User
    participant API as Presentation API
    participant App as Application
    participant Repo as Reconciliation Repository

    User->>API: GET /api/reports/condominiums/{condominiumId}/reconciliation/{billingPeriodId}
    API->>App: GetCondominiumReconciliationQuery
    App->>Repo: Load condominium, properties, meters, readings
    Repo-->>App: Reconciliation data
    App->>App: Compare main meter vs sub-meters
    App->>App: Flag missing readings, high consumption, bill mismatch
    App-->>API: Reconciliation DTO
    API-->>User: Difference %, suspect properties, explanation
```

## Local deployment view

```mermaid
flowchart TB
    subgraph DockerCompose[Docker Compose]
        API[api service]
        Worker[worker service]
        MinIO[minio service]
        Volume[(shared /data volume)]
    end

    API --> Volume
    Worker --> Volume
    API --> MinIO
    Worker --> MinIO
```

## Notes for contributors

- Run both **Presentation** and **Worker** when testing queued background flows.
- Docker Compose is the simplest way to get a shared database, queue path, and object storage locally.
- The current implementation already reflects the intended separation between synchronous API writes and slower background work.
