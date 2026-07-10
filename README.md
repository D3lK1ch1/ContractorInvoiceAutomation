# Contractor Invoice Automation

A .NET portfolio project: a practical invoicing workflow for a solo Australian contractor — clients, invoices with hourly/fixed-price line items, GST-aware totals, payment tracking, and a follow-up workflow, with AI assistance for wording (never for sending or deciding).

Full product/technical spec: [SPEC.md](SPEC.md). Build-by-build history: [CHANGELOG.md](CHANGELOG.md).

## Status

Early build, but the core loop is walkable end to end in the browser: set up your contractor profile, add a client, create an invoice with mixed hourly/fixed-price line items, and export it to CSV. Domain logic (calculation, status, invoice numbering, payments) has passing unit tests. PDF invoice generation and a styled/formatted export are not built yet — CSV export currently produces plain, unstyled data (correct content, no visual formatting) — see SPEC.md section 14 for the current "next unit."

## Tech stack

.NET 10, ASP.NET Core / Blazor Web App, EF Core with SQLite, xUnit.

## Project structure

- `src/ContractorInvoicing.Domain` — entities and business logic (calculation services). No dependencies on EF Core or the web layer.
- `src/ContractorInvoicing.Infrastructure` — EF Core `DbContext`, SQLite persistence, migrations, and services that need direct DB access (e.g. `InvoiceCsvService`).
- `src/ContractorInvoicing.Web` — Blazor Web App (Server interactivity). Pages: Profile, Clients, New Invoice, Invoices (with CSV export).
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

To run the app:

```
cd src/ContractorInvoicing.Web
dotnet run
```

Then open the printed URL and go to **Your Profile** first (invoice GST calculation depends on it), then **Clients**, then **New Invoice**.
