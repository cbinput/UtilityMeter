# Plan 03: Readings, evidence, and consumption

## Goal
Implement the basic reading workflow: capture a reading, attach evidence, calculate consumption, and track the source of truth.

## Tasks

### 1. Add reading creation flow
- Create reading command and validation
- Ensure reading belongs to a property and meter
- Support manual reading submission and source tracking

### 2. Add evidence flow
- Upload meter photo or bill PDF
- Save file to object storage
- Record hash, storage key, type, and metadata
- Link evidence to the reading

### 3. Calculate consumption
- Consumption is derived, not manually entered
- formula: current reading - previous reading
- handle missing previous reading explicitly

### 4. Support reading lifecycle status
- pending, valid, warning, mismatch, etc. as domain states
- keep traceability for original reading and produced consumption

### 5. Expose API endpoints
- create reading
- get readings by meter / property / billing period
- attach evidence

## Acceptance criteria
- A valid reading can be persisted with evidence and calculated consumption
- The system distinguishes between reading data and derived consumption
- The API is thin and delegates to MediatR handlers

## Notes
This phase is the MVP baseline. It should work before anomaly detection and reconciliation are added.
