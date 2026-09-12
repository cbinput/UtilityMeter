# Plan 05: Condominium reconciliation and proration

## Goal
Model the condominium scenario where a main meter is compared against the sum of sub-meters and each property’s consumption contributes to a shared allocation.

## Tasks

### 1. Add condominium-level structures
- main meter
- property sub-meters
- shared billing period context

### 2. Add reconciliation logic
- compute main meter total vs sum of sub-meters
- calculate percentage difference
- identify suspect properties for analysis

### 3. Add anomaly grouping
- properties with high consumption
- missing readings
- bill mismatches
- unusual consumption patterns

### 4. Add reporting outputs
- reconciliation summary
- list of suspect properties
- difference percentage and explanation

### 5. Add tests
- exact-match reconciliation
- mismatched sub-meter totals
- missing readings in shared period

## Acceptance criteria
- The system can compare a main meter against the sum of property meters
- Reconciliation produces a percentage difference and suspect list
- The logic is separate from the reading validation flow

## Notes
This is the domain-specific complexity that makes the system useful for condominium operations rather than simple household utility tracking.
