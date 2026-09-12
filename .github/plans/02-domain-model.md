# Plan 02: Domain model and core entities

## Goal
Define the core domain objects and relationships needed to represent utility data without collapsing key concepts together.

## Tasks

### 1. Define the main domain hierarchy
- Organization / Tenant
- Condominium
- Property
- Meter
- BillingPeriod
- Reading
- Evidence
- Consumption
- Validation
- Alert

### 2. Create resource-agnostic types
- Extensible resource type enum or equivalent model
- Keep resource-specific logic out of a single implementation

### 3. Model billing periods
- States: Open, Collecting, Validating, Closed
- Closed periods must be immutable

### 4. Define reading and evidence records
- Reading includes value, date, unit, source, status
- Evidence includes immutable storage reference, hash, and metadata
- Evidence should be stored outside PostgreSQL in object storage

### 5. Model alert and validation outcomes
- Separate structural invalid input from business warnings and alerts
- Do not reject valid readings simply because they are unusual

## Acceptance criteria
- The domain model reflects the system’s conceptual separation
- Billing periods and readings can be tracked independently
- Evidence is represented as immutable metadata rather than raw file storage in the database

## Notes
The domain model is the heart of the project. This plan must be completed carefully before building API endpoints or worker jobs.
