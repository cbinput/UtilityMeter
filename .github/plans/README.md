# UtilityMeter implementation plan

This directory contains the incremental delivery plan for the utility consumption system described in [../copilot-instructions.md](../copilot-instructions.md).

## Plan overview

1. [01-foundation-and-architecture.md](01-foundation-and-architecture.md) – project setup and architectural baseline
2. [02-domain-model.md](02-domain-model.md) – core entities and structure
3. [03-readings-evidence-and-consumption.md](03-readings-evidence-and-consumption.md) – readings, evidence, and consumption logic
4. [04-validation-alerts-and-reporting.md](04-validation-alerts-and-reporting.md) – validation, alerts, and reporting outcomes
5. [05-condominium-proration.md](05-condominium-proration.md) – main meter and sub-meter reconciliation
6. [06-worker-storage-and-ops.md](06-worker-storage-and-ops.md) – background jobs, storage, and deployment setup

## Working principle

Deliver the system in small slices, each one testable and aligned with the CleanMinimalApi template. Each plan should keep the domain separation intact:

- Measurement
- Consumption
- Billing
- Proration
- Audit / Analysis

The first goal is to get a working, traceable reading pipeline before adding advanced OCR, dashboards, and external integrations.
