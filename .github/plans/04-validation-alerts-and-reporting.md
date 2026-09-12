# Plan 04: Validation, alerts, and reporting outcomes

## Goal
Implement the business rules that flag abnormal or missing readings without conflating them with input validation failures.

## Tasks

### 1. Add validation rules
- numeric and required field checks
- meter belongs to property
- billing period is open or valid for update
- evidence required when applicable

### 2. Add business-rule alerts
- current reading lower than previous reading
- abnormal high consumption versus historical average
- missing reading for a period
- resident-company mismatch

### 3. Create alert domain model
- alert severity, type, description, linked meter/property/reading
- keep alert records independent from invalid form submissions

### 4. Add reporting views
- list of pending readings
- list of abnormal readings
- monthly summary with mismatch information

### 5. Add tests for business outcomes
- invalid form data fails validation
- valid but unusual readings produce alerts instead of rejection

## Acceptance criteria
- The app records abnormal situations as domain outcomes
- Business-rule alerts are visible in reporting logic
- Validation and alerting behavior are separated cleanly

## Notes
This is where the product moves beyond basic CRUD and begins supporting operational monitoring and auditability.
