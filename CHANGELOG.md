# Changelog

All notable changes to this project are documented here. Format loosely
follows [Keep a Changelog](https://keepachangelog.com/).

## 10/7/2026 v0.1.0 - First walkable UI loop: profile → client → invoice → CSV

### Added
- Contractor Profile page (`/profile`) — business name, ABN (validated live
  against `AbnValidator`), email, payment details, GST-registered flag,
  default payment terms. Single-row, edit-in-place.
- Clients page (`/clients`) — create and list clients, including a new `Abn`
  field (optional, validated against the ATO check-digit algorithm when
  provided — not every client has given one yet).
- Invoice creation page (`/invoices/new`) — pick a client, dates default from
  payment terms, add line items with a per-line **Hourly / Fixed Price**
  toggle (Hourly shows Hours + Rate; Fixed Price shows a single Amount field,
  no hours), totals recalculate live via the existing
  `InvoiceCalculationService`, saves as Draft and generates the invoice
  number via the existing `InvoiceNumberService`.
- Invoices list page (`/invoices`) with per-invoice and bulk CSV export
  links.
- `InvoiceCsvService` (Infrastructure) with two distinct export shapes:
  a flat one-row-per-invoice bulk export (`ExportAllAsync`, the ATO
  record-keeping backup path from section 12) and a structured
  single-invoice document export (`ExportInvoiceAsync` — FROM/BILL TO with
  ABNs, invoice number, dates, period covered, due terms, line-item table,
  GST breakdown or not-registered note, payment details — section 6.3).
  Redesigned mid-session after comparing output against a real reference
  invoice; the two exports serve different purposes and were wrongly
  conflated into one shape at first.
- Two minimal API endpoints for CSV download (`/export/invoices.csv`,
  `/export/invoices/{id}.csv`) — Blazor Server needs a real HTTP response to
  trigger a browser download. Filenames use the actual invoice number
  (sanitized against invalid filesystem characters), not the database ID.
- `Invoice.Terms` now populated on save (e.g. "Within 14 days of issue"),
  previously an unused field.

### Changed
- `Program.cs`: switched `AddDbContext` → `AddDbContextFactory`. Blazor
  Server interactive components hold their connection open for the session
  duration; a single shared scoped `DbContext` across that whole lifetime is
  a known footgun. The factory creates short-lived contexts per operation.
- Removed `Client.Notes` — nothing in the invoicing flow ever reads it, and
  it wasn't shown anywhere a user would see it either; decided not worth
  keeping as a write-only field. Migration drops the column.
- Currency display pinned to `en-AU` culture explicitly (`CultureInfo`)
  instead of relying on the server's ambient locale, which was rendering
  AUD amounts as Malaysian Ringgit on one dev machine.

### Fixed
- `appsettings.json` had silently lost its `ConnectionStrings.Default` entry
  as a side effect of an unrelated earlier commit (`be2748f`) — restored.
  The missing connection string caused SQLite to fall back to an empty,
  schema-less temp database per connection, surfacing as
  `SqliteException: no such table`.

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
