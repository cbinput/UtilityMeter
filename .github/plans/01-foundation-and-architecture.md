# Plan 01: Foundation and architecture

## Goal
Set up the repo structure and baseline patterns required by the utility-meter system without changing the existing CleanMinimalApi architecture.

## Tasks

### 1. Confirm template conventions
- Keep the existing 3-layer pattern: Presentation, Application, Infrastructure
- Reuse MediatR, FluentValidation, AutoMapper, Serilog, Swashbuckle
- Preserve current solution structure and .NET conventions

### 2. Add required project support
- Add a Worker project for async background processing
- Keep worker logic thin; it dispatches Application commands/queries
- Define integration boundaries for queue and storage

### 3. Define application boundaries
- Create feature folders: Properties, Meters, Readings, Evidence, BillingPeriods, Alerts, Reports
- Keep repositories and interfaces in Application
- Keep implementations internal in Infrastructure

### 4. Set up baseline tests
- Add unit tests for command validation and core domain rules
- Add integration tests for persistence and API behavior

## Acceptance criteria
- Solution compiles with the added Worker project
- Core skeleton matches the template’s layered convention
- Application features are organized by domain area rather than by technical concern
- Testing strategy exists before feature work begins

## Notes
This is a structural phase. It should not implement final business logic yet; it should create the safe base for the actual utility domain.
