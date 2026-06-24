# Changelog

All notable changes to this project are documented here. Format loosely
follows [Keep a Changelog](https://keepachangelog.com/).

## 24/6/2026 v0.0.0 - Scaffolding the app

### Added
- Solution scaffold: 4 projects (`Domain`, `Infrastructure`, `Web`, `Tests`).
- Core domain entities: `ContractorProfile`, `Client`, `Invoice`,
  `InvoiceLineItem`, `Payment`, `AutomationTask`.
- EF Core `DbContext` with SQLite persistence; initial migration creating all
  6 tables, foreign keys, and indexes.
- `InvoiceCalculationService` — subtotal/GST/total calculation (10% GST on
  taxable lines, only when the contractor is GST-registered), with 3 unit
  tests covering subtotal summing, mixed taxable/non-taxable lines, and the
  GST-not-registered case.

### Changed
- Dropped structured address fields (`AddressLine1/2`, `City`, `State`,
  `Postcode`, `Country`) from `ContractorProfile` and `Client` — out of scope
  for v1.
- Added `DefaultHourlyRate` to `Client` (pre-fills new invoice line items) and
  `PeriodStart`/`PeriodEnd` to `Invoice` (the billing period an invoice
  covers), based on cross-checking a real reference invoice against the spec.
- Added CSV export to v1 scope, for archiving invoices into a spreadsheet —
  no live Google Sheets/Drive API integration.

### Security
- Pinned `SQLitePCLRaw.bundle_e_sqlite3` to 3.0.3 — the version EF Core
  10.0.9 pulls in transitively (2.1.11) has a known high-severity NuGet
  advisory (NU1903).
