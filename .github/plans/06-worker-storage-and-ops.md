# Plan 06: Worker, storage, and deployment

## Goal
Handle heavy processing asynchronously and prepare the app for self-hosted deployment without overengineering the system.

## Tasks

### 1. Add background worker infrastructure
- create Worker project
- implement background service and job dispatcher
- keep business logic in Application

### 2. Add storage abstraction
- define IObjectStorage contract in Application
- implement MinIO-backed storage in Infrastructure
- keep raw evidence files outside the database

### 3. Add background job queue abstraction
- define IBackgroundJobQueue in Application
- implement with System.Threading.Channels
- allow replacement with external queue later

### 4. Add async job types
- OCR / extraction jobs
- report generation jobs
- anomaly analysis jobs
- export jobs

### 5. Wire up docker-compose environment
- PostgreSQL
- MinIO
- API app
- Worker app

## Acceptance criteria
- Heavy work runs outside the API request lifecycle
- Evidence exists in persistent object storage
- The stack is deployable in a simple self-hosted environment
- Queue and storage abstractions can be swapped later without an architecture rewrite

## Notes
This phase finishes the operational backbone and makes the MVP ready for realistic deployment without premature complexity.
