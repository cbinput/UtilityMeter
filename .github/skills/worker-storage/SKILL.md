---
name: worker-storage
description: >-
  Change asynchronous processing, background jobs, or evidence storage in this
  project. Use when a task touches the Worker host, queued jobs, MinIO object
  storage, shared runtime configuration, or any flow that must stay fast in the
  API and complete later in the background.
---

# Skill: Worker and Storage

Use this skill for OCR, anomaly analysis, report generation, exports, and evidence-file handling.

## Execution model

- HTTP endpoints should accept fast work only.
- Slow work must be represented as a background job and processed by `src/Worker`.
- The Worker should dispatch back into Application commands or queries through MediatR instead of duplicating business logic.

## Queue rules

- Keep the abstraction at `IBackgroundJobQueue`.
- The current self-hosted implementation uses in-process channels plus JSON persistence under `BACKGROUND_JOB_QUEUE_PATH`.
- API and Worker must share queue and persistence configuration when testing end-to-end flows.
- Failures should return work to the pending queue path instead of silently dropping it.

## Storage rules

- Keep the abstraction at `IObjectStorage`.
- Evidence files belong in object storage, not the relational database.
- The current self-hosted provider is MinIO.
- The MinIO implementation creates the configured bucket on demand, so avoid adding separate provisioning assumptions inside handlers.

## Shared local-run expectations

- For manual runs, Presentation and Worker should point to the same database and `BACKGROUND_JOB_QUEUE_PATH`.
- Docker Compose is the preferred full-stack local setup when validating queue or storage flows.

## Files worth checking first

- `/src/Worker/QueuedBackgroundWorker.cs`
- `/src/Infrastructure/BackgroundJobs/InMemoryBackgroundJobQueue.cs`
- `/src/Infrastructure/Storage/MinioObjectStorage.cs`
- `/docker-compose.yml`
- `/README.md`
- `/docs/en/architecture.md`
