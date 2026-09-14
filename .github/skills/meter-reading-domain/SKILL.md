---
name: meter-reading-domain
description: >-
  Work on utility metering behavior without breaking the repository's domain
  model. Use when a task involves readings, evidence, consumption,
  billing-period rules, alerts, company-versus-resident comparisons, or
  condominium reconciliation.
---

# Skill: Meter Reading Domain

This repository treats measurement as first-class data. Preserve traceability at all times.

## Never collapse these concepts

`Measurement -> Consumption -> Billing -> Proration -> Audit/Analysis`

That separation must stay visible in naming, storage, API behavior, and reporting.

## Domain invariants

- A `Reading` belongs to a `Meter`, `Property`, and `BillingPeriod`.
- `Consumption` is calculated from readings and is never manually entered.
- `Evidence` is immutable support for a reading or bill and stores references such as `storageKey` and `hash`, not raw files in PostgreSQL.
- A property can legitimately have no reading for a billing period.
- A resident reading and a company reading may both exist and disagree without invalidating each other.
- Resource handling must remain extensible through `ResourceType`; avoid hardcoded water-only, gas-only, or electricity-only logic.

## Business-rule guidance

- Structural request problems belong in FluentValidation.
- Domain anomalies must be recorded as outcomes or alerts, not rejected as malformed input.
- Preserve support for:
  - decreased readings unless rollover or replacement explains them
  - abnormal consumption versus historical behavior
  - missing readings for a period
  - mismatch between resident and utility-company readings
  - condominium reconciliation between a main meter and sub-meters

## Model changes should respect

- immutable evidence trails
- closed billing periods being immutable
- auditable comparisons between company and resident data
- reporting that can identify suspect properties instead of hiding inconsistencies

## Files worth checking first

- `/.github/copilot-instructions.md`
- `/docs/en/architecture.md`
- `/src/Application/Readings`
- `/src/Application/Evidence`
- `/src/Application/Reports`
- `/src/Application/Alerts`
