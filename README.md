# Contractor Invoice Automation

A .NET portfolio project: a practical invoicing workflow for a solo Australian contractor — clients, invoices with hourly/fixed-price line items, GST-aware totals, payment tracking, and a follow-up workflow, with AI assistance for wording (never for sending or deciding).

Full product/technical spec: [SPEC.md](SPEC.md). Build-by-build history: [CHANGELOG.md](CHANGELOG.md).

## Status

Early build. Domain model, SQLite persistence, and the invoice calculation logic (subtotal/GST total) exist with passing unit tests. No UI screens yet — see SPEC.md section 14 for the current "next unit."

## Tech stack

.NET 10, ASP.NET Core / Blazor Web App, EF Core with SQLite, xUnit.

## Project structure

- `src/ContractorInvoicing.Domain` — entities and business logic (calculation services). No dependencies on EF Core or the web layer.
- `src/ContractorInvoicing.Infrastructure` — EF Core `DbContext`, SQLite persistence, migrations.
- `src/ContractorInvoicing.Web` — Blazor Web App (Server interactivity).
- `tests/ContractorInvoicing.Tests` — xUnit tests for the Domain layer.

## Running it

```
dotnet build
dotnet test
```

To create/update the local SQLite database:

```
cd src/ContractorInvoicing.Web
dotnet ef database update --project ../ContractorInvoicing.Infrastructure --startup-project .
```
