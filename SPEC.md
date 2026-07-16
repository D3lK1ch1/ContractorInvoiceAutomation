# Contractor Invoice Automation - Product & Technical Spec

## 1. Product Goal

Build a .NET portfolio project that is useful for a real contractor workflow: creating invoices, tracking payments, monitoring outstanding income, and preparing follow-up actions.

The app should feel like practical business software, not a tutorial CRUD project. It should demonstrate finance workflow thinking, clean .NET engineering, and useful AI-assisted automation for freelancers and small service businesses.

## 2. Target User

Primary user:

- A solo contractor or freelancer issuing invoices to clients.
- Works with a small number of clients at first.
- Wants a clear view of issued invoices, payments, overdue items, and next actions.
- May not want a full accounting system yet.

Secondary portfolio audience:

- Potential clients who need lightweight internal finance/admin tooling.
- Hiring managers or technical reviewers assessing .NET, business logic, data modelling, and workflow automation skills.

## 3. V1 Scope

V1 should focus on a complete single-user contractor invoicing workflow.

Included:

- Contractor profile setup.
- Client management.
- Invoice creation.
- Invoice line items for hourly, fixed-price, and mixed work.
- Australian invoice fields such as ABN, AUD, GST-aware totals, issue date, due date, and payment terms.
- Invoice status tracking.
- Payment recording.
- Styled invoice export (.xlsx) — a client-ready document, not just a data dump. Replaces PDF generation; see §14 decision log, 2026-07-15.
- CSV export of invoice data (for archiving in a Google Sheets tab or similar — no live API integration in v1).
- Dashboard for totals, overdue invoices, and next actions.
- Practical AI assistance for wording, follow-up drafts, and anomaly checks.

## 4. Out of Scope for V1

The following should be deliberately excluded from v1 to keep the first version focused:

- Multi-tenant SaaS accounts.
- Stripe or card payment collection.
- Automatic email sending.
- PDF invoice generation (superseded by styled .xlsx export — see §14 decision log, 2026-07-15).
- Bank feed integration.
- Live Google Sheets / Drive API write-back (v1 covers CSV export only; the user imports/pastes it into Sheets manually).
- Full accounting ledger.
- BAS/tax filing features.
- OCR extraction from uploaded invoices.
- Multi-country tax compliance.
- Mobile app.
- Role-based team permissions.
- Seed demo data (dropped 2026-07-15 — the app is used live with real data from the start, not staged for portfolio demo; see §14 decision log).

These can be discussed as future roadmap items after the core workflow is working.

## 5. Australian Invoice Requirements

Default region: Australia.

Default currency: AUD.

The invoice model should support:

- Contractor or business name.
- ABN field.
- GST registration flag.
- Business email.
- Payment details.
- Client name and business details.
- Invoice number.
- Issue date.
- Due date.
- Period covered (work period start/end date — optional, mainly for hourly work).
- Payment terms.
- Line item description.
- Quantity or hours.
- Unit rate.
- Subtotal.
- GST amount when enabled.
- Total invoice amount.
- Amount paid.
- Outstanding balance.

GST handling:

- If GST is disabled, invoice totals should show subtotal and total only.
- If GST is enabled, invoice totals should calculate 10% GST on taxable line items by default.
- The app should avoid giving tax advice. Labels should be functional, not advisory.

## 6. Core Workflows

### 6.1 Contractor Profile Setup

The user enters reusable invoice details once:

- Business or contractor name.
- ABN.
- Email.
- Payment details.
- GST registration flag.
- Default payment terms, such as 7, 14, or 30 days.

ABN validation: the app must validate the ABN format using the ATO's published check digit algorithm before saving. A malformed ABN on every issued invoice is a silent, compounding error. Validation should be a pure function in the Domain layer with its own unit tests.

### 6.2 Client Management

The user can create, edit, and archive clients.

Client fields:

- Client contact name.
- Client business name.
- Email.
- ABN (optional — needed on invoices for business clients; not every client provides one).
- Default payment terms override.
- Default hourly rate (optional — pre-fills new line items for this client; a specific invoice can still override it, e.g. a project with a different agreed rate). This is a pre-fill only, not an enforced billing model — a client can be invoiced hourly on one invoice and at a flat project rate on another, and different clients can be on entirely different pay structures (e.g. a fixed hourly rate vs. a lump sum billed once at project completion).
- Next invoice sequence (integer, default 1 — the number to use when generating the next invoice for this client). When registering a client who already has invoices outside the app, set this to the correct next number so the sequence picks up from there rather than restarting at 001.

### 6.3 Invoice Creation

The user can create an invoice from a client.

Invoice behaviour:

- Auto-generate invoice number using the format `{ClientName} Invoice {NNN}` where NNN is a 3-digit zero-padded per-client sequence (e.g. `Acme Invoice 004`). The sequence is per-client and independent — a new client starts at 001 regardless of other clients' invoice counts.
- Default issue date to today.
- Default due date from payment terms.
- Optionally set period covered (start/end date of the billing period).
- Add line items, pre-filling rate from the client's default hourly rate when set.
- Calculate totals immediately.
- Save as draft.
- Mark as sent.
- Export styled invoice document (.xlsx) — client-ready, with formatting CSV can't carry.
- Export CSV (spreadsheet-compatible, for archiving in Google Sheets or similar).

Line item types:

- Hourly work: description, hours, rate.
- Fixed-price work: description, quantity, unit price.
- Mixed line items should be supported by the same line item model. This means an invoice's billing style is decided per line item, not fixed by client or invoice type — a contractor paid hourly at minimum wage and a contractor paid a single project fee at completion are both just different combinations of quantity and unit price on the same model, with no separate "engagement type" concept needed.

### 6.4 Payment Tracking

The user can record one or more payments against an invoice.

Payment fields:

- Payment date.
- Amount.
- Method.
- Reference.
- Notes.

Invoice status should update based on payment state:

- Draft: not sent.
- Sent: sent but not fully paid and not overdue.
- Partially Paid: payments exist but outstanding balance remains.
- Paid: outstanding balance is zero.
- Overdue: due date has passed and outstanding balance remains.

### 6.5 Follow-Up Workflow

The app should generate recommended follow-up actions without sending automatically.

Examples:

- Reminder due 3 days before due date.
- Reminder due on due date.
- Overdue follow-up 7 days after due date.
- Follow-up again 14 days after due date if still unpaid.

The user should be able to mark follow-up tasks as done or ignored.

## 7. Data Model

Suggested core entities:

### ContractorProfile

- Id
- BusinessName
- Abn
- Email
- PaymentDetails
- IsGstRegistered
- DefaultPaymentTermsDays
- CreatedAt
- UpdatedAt

### Client

- Id
- ContactName
- BusinessName
- Email
- Abn (nullable)
- DefaultPaymentTermsDays
- DefaultHourlyRate (nullable)
- NextInvoiceSequence (int, default 1 — incremented after each invoice number is generated for this client)
- IsArchived
- CreatedAt
- UpdatedAt

### Invoice

- Id
- ClientId
- InvoiceNumber
- IssueDate
- DueDate
- PeriodStart (nullable — billing period start, mainly for hourly work)
- PeriodEnd (nullable — billing period end)
- Status
- Notes
- Terms
- Subtotal
- GstAmount
- Total
- AmountPaid
- OutstandingBalance
- CreatedAt
- UpdatedAt

### InvoiceLineItem

- Id
- InvoiceId
- Description
- Quantity
- UnitPrice
- IsTaxable
- LineSubtotal
- GstAmount
- LineTotal
- SortOrder

### Payment

- Id
- InvoiceId
- PaidAt
- Amount
- Method
- Reference
- Notes
- CreatedAt

### AutomationTask

- Id
- InvoiceId
- TaskType
- ScheduledFor
- Status
- DraftMessage
- CompletedAt
- CreatedAt

## 8. AI & Automation Features

V1 should include practical AI assistance, not autonomous finance decisions.

AI features:

- Turn rough work notes into polished invoice line descriptions.
- Draft polite payment reminder messages.
- Draft overdue follow-up messages.
- Suggest next actions based on invoice state.
- Flag unusual invoice totals compared with previous invoices.

AI constraints:

- The user must approve all AI-generated text.
- AI should not send emails automatically in v1.
- AI should not provide tax or legal advice.
- AI prompts should avoid sending unnecessary personal or sensitive data.
- The AI service should be behind an interface so providers can be swapped later.

Suggested service interface:

```csharp
public interface IInvoiceAiAssistant
{
    Task<string> ImproveLineItemDescriptionAsync(string roughNotes);
    Task<string> DraftPaymentReminderAsync(InvoiceReminderContext context);
    Task<IReadOnlyList<string>> SuggestNextActionsAsync(InvoiceWorkflowContext context);
    Task<IReadOnlyList<string>> DetectInvoiceAnomaliesAsync(InvoiceAnomalyContext context);
}
```

Non-AI automation should calculate follow-up tasks deterministically from due dates and payment state.

## 9. Dashboard Requirements

The dashboard should be the first screen after launch.

It should show:

- Total invoiced this month.
- Total paid this month.
- Outstanding balance.
- Overdue balance.
- Number of overdue invoices.
- Upcoming due invoices.
- Recent payments.
- Client income breakdown.
- Monthly income trend.
- Recommended next actions.

The design should be quiet, practical, and business-focused. It should prioritise scanning, comparison, and repeated use over decorative presentation.

## 10. Technical Stack

Recommended stack:

- .NET 10.
- ASP.NET Core.
- Blazor Web App.
- Entity Framework Core.
- SQLite for local development.
- Option to migrate to PostgreSQL later.
- A .NET library for generating styled .xlsx workbooks.
- Built-in dependency injection.
- xUnit or NUnit for tests.
- Playwright or bUnit for selected UI tests if the UI grows.

Architecture:

- Modular monolith.
- Keep domain logic separate from UI components.
- Keep invoice calculation logic testable without the database.
- Keep AI integration behind an interface.
- Keep xlsx export behind a service interface.

Suggested service boundaries:

- InvoiceCalculationService.
- InvoiceNumberService.
- InvoiceStatusService.
- PaymentService.
- AutomationTaskService.
- InvoiceXlsxService.
- InvoiceCsvService.
- InvoiceAiAssistant.

## 11. Testing Plan

Unit tests:

- Invoice subtotal calculation.
- GST calculation when GST is enabled.
- No GST calculation when GST is disabled.
- Partial payment balance calculation.
- Full payment status transition.
- Overdue status transition.
- Reminder task scheduling.
- Invoice number generation.
- AI prompt-building logic without calling a live AI provider.

Integration tests:

- Create client.
- Create invoice with line items.
- Record payment.
- Verify dashboard totals.
- Generate styled invoice .xlsx export from saved invoice data.
- Generate invoice CSV export from saved invoice data.

UI acceptance scenarios:

- User creates contractor profile.
- User creates client.
- User creates invoice.
- User exports invoice as .xlsx.
- User marks invoice as sent.
- User records partial payment.
- User records full payment.
- User sees paid invoice leave overdue/outstanding lists.
- User generates a reminder draft for an overdue invoice.

## 12. Acceptance Criteria

V1 is complete when:

- A user can create a contractor profile.
- A user can create and manage clients.
- A user can create an invoice with multiple line items.
- The app correctly calculates subtotal, GST, total, paid amount, and outstanding balance.
- The app can export a professional, styled invoice document (.xlsx).
- The app can export invoice data as CSV for spreadsheet archiving.
- The user can record one or more payments.
- Invoice statuses update correctly.
- The dashboard reflects invoice and payment state.
- The app generates follow-up task recommendations.
- AI can draft invoice wording and payment reminders through a replaceable service.
- Core business logic has automated tests.
- A full CSV export of all invoices (not just per-invoice) is producible on demand. This is the defined backup path: the user can export the complete invoice history at any time without manual per-invoice steps. Required because ATO record-keeping obligations run for 5 years and the app stores data in a local SQLite file with no automatic backup.

## 13. Future Roadmap

Potential v2 features:

- Email sending with user approval.
- Stripe payment links.
- Bank transaction import and reconciliation.
- Reuse ideas from the existing bank transaction analyser project.
- Recurring invoices.
- Quote-to-invoice workflow.
- Work log import (turn dated work-log entries into invoice line items — see 13.1).
- Expense tracking.
- Accountant export.
- BAS preparation summary.
- Multi-user teams.
- Multi-tenant SaaS mode.
- OCR extraction from receipts or supplier invoices.
- Hosted deployment with authentication.
- CSV invoice seeding import: allow the user to import a CSV of past invoices (from Google Sheets or similar) to seed real invoice records in the app — not just a starting counter. This preserves the full invoice history before the app existed, so records go back to Invoice 001 and the dashboard reflects true totals. Design separately once the invoice creation UI exists so the import maps to a known, working data shape.
- Work log document import with AI assistance: import a freeform work log (date, role, hours, notes) and use AI to improve the wording of notes before they become line item descriptions. The import groups entries by role, sums hours, and drafts line items — the user reviews and edits before saving. See 13.1 for the existing design sketch; this note adds the AI wording improvement step to that flow.

### 13.1 Work Log Import — Design Sketch

Deferred. This needs `InvoiceCalculationService` and the invoice creation workflow to exist first — there is nothing to import *into* yet. Documented now so the idea isn't lost, not scheduled as the next unit.

Problem it solves: line items currently require the user to already know total hours per role for the billing period. In practice hours are recorded day-by-day in a running log, then need to be summed per role before they become a line item.

New entity:

#### WorkLogEntry

- Id
- ClientId (which client this entry's work was for)
- EntryDate
- Role (free text, matches the description used on the invoice line item, e.g. "Project Management", "Engineering")
- Hours
- Notes
- InvoiceLineItemId (nullable — set once the entry has been rolled into a billed line item)
- CreatedAt

Workflow:

- The user records work log entries as they go (date, client, role, hours, notes).
- When creating an invoice, the user picks the client and a date range and imports that client's unbilled entries for that range.
- The app groups entries by role and sums hours into one draft line item per role.
- If a day in the range has a role entry with no hours recorded, the app flags it instead of treating it as zero or guessing a value.
- Before presenting the draft line items, the AI assistant offers improved wording for each line item description based on the raw notes — the user can accept, edit, or ignore the suggestion.
- The user reviews and can edit the generated line items before saving the invoice — import only prepares a draft, it never creates or sends an invoice on its own.

## 14. Build Progress

Status as of 2026-06-29.

### Done

- Solution scaffolded: `ContractorInvoicing.sln` with 4 projects.
  - `src/ContractorInvoicing.Domain` — class library, no dependencies.
  - `src/ContractorInvoicing.Infrastructure` — class library, references Domain.
  - `src/ContractorInvoicing.Web` — Blazor Web App (Server interactivity), references Domain + Infrastructure.
  - `tests/ContractorInvoicing.Tests` — xUnit, references Domain.
- Core entities added under `src/ContractorInvoicing.Domain/Entities/`, matching section 7 field-for-field:
  - `ContractorProfile`, `Client`, `Invoice`, `InvoiceLineItem`, `Payment`, `AutomationTask`.
  - Enums: `InvoiceStatus`, `AutomationTaskType`, `AutomationTaskStatus`.
- `.gitignore` added (bin/obj/SQLite files). Repo initialized with `git init`; 4 commits on `main` (scaffold → DbContext → calculation service → docs).
- `dotnet build` succeeds, 0 warnings, 0 errors.
- No tests yet — entities are plain POCOs with no behavior, so nothing meaningful to test until calculation/status logic exists.

### Open judgment calls to confirm later

- `ContractorProfile.Country` / `Client.Country` default to `"Australia"`.
- `Client.DefaultPaymentTermsDays` is `int?` (override, falls back to contractor default when null); `ContractorProfile.DefaultPaymentTermsDays` is `int` defaulting to 14.
- Dates use `DateOnly` (IssueDate, DueDate, PaidAt, ScheduledFor); timestamps use `DateTime` (CreatedAt/UpdatedAt).

### Spec decisions — 2026-06-24

Reviewed two real reference files (an actual invoice and a freeform work-log
document) against this spec, and checked three GitHub repos
found while searching for existing open-source alternatives
(`microsoft/Azure-Invoice-Process-Automation-Solution-Accelerator`,
`makerever/rever`, `kalamdreamlabs/kdl-starter-kit`) to decide build-from-scratch
vs. fork. None fit: the first two solve accounts *payable* (processing received
invoices), not accounts *receivable* (issuing them); one carries an AGPL
copyleft license; none are in .NET. Decision: keep building from scratch.

Resulting spec changes (reflected above, not yet in code — see "Next unit"):

- Added `Client.DefaultHourlyRate` (single flat rate, no per-role split — no
  current client needs per-role rates; revisit only if one does).
- Added `Invoice.PeriodStart`/`PeriodEnd` (nullable) — the real invoice shows a
  "period covered" range distinct from issue/due date; also the date range
  Work Log Import (13.1) will eventually use.
- Added CSV export to v1 scope, and `ClientId` to the `WorkLogEntry` sketch in
  13.1, so logged hours can be filtered/imported per client now that more than
  one client is in scope. Live Google Sheets API write-back stays out of scope
  (section 4) — CSV export is enough for manual archiving into a Sheets tab.
- Work Log Import (13.1) stays deferred; build order below is unchanged.
- Dropped structured address fields entirely (`AddressLine1/2`, `City`, `State`,
  `Postcode`, `Country`) from both `ContractorProfile` and `Client`, and removed
  "Business address"/"Address"/"Billing address" from sections 5, 6.1, 6.2 —
  not used anywhere in the codebase outside the two entity files, confirmed via
  grep before removing.

### Done — 2026-06-24

- Synced entities to match the spec decisions above: added `DefaultHourlyRate`
  to `Client.cs`, added `PeriodStart`/`PeriodEnd` to `Invoice.cs`, removed
  `AddressLine1/2`/`City`/`State`/`Postcode`/`Country` from both
  `ContractorProfile.cs` and `Client.cs` (confirmed unused elsewhere via grep
  before removing).
- `dotnet build` succeeds, 0 warnings, 0 errors.

### Done — 2026-06-24 (continued)

- EF Core `DbContext` + SQLite setup:
  - Added `Microsoft.EntityFrameworkCore.Sqlite` to `ContractorInvoicing.Infrastructure`,
    `Microsoft.EntityFrameworkCore.Design` to `ContractorInvoicing.Web`. Pinned
    `SQLitePCLRaw.bundle_e_sqlite3` to 3.0.3 directly — the version EF Core 10.0.9
    pulls in transitively (2.1.11) has a known high-severity NuGet advisory
    (NU1903); 3.0.3 resolves clean.
  - Added `ContractorInvoicingDbContext` in
    `src/ContractorInvoicing.Infrastructure/Data/`, with a `DbSet<T>` per entity.
    No fluent configuration needed — SQLite's EF Core provider stores `decimal`
    as TEXT by default (avoids floating-point rounding for money) and supports
    `DateOnly`/`DateOnly?` natively, and every relationship follows EF Core's
    default by-convention mapping (`XxxId` + nav property), so there was nothing
    non-standard to configure.
  - Wired it into DI in `Program.cs` (`AddDbContext` + `UseSqlite`), connection
    string in `appsettings.json` (`Data Source=contractorinvoicing.db`).
  - Updated the global `dotnet-ef` tool from 7.0.5 → 10.0.9 to match the package
    versions (the old tool predates .NET 10 and would likely have failed or
    behaved unpredictably against 10.0.9 packages).
  - Generated and applied migration `InitialCreate` — created
    `src/ContractorInvoicing.Web/contractorinvoicing.db` with all 6 tables, FKs,
    and indexes. Already covered by `.gitignore` (`*.db`).
- `dotnet build` succeeds, 0 warnings, 0 errors.

### Done — 2026-06-24 (continued)

- `InvoiceCalculationService`: added `IInvoiceCalculationService`/`InvoiceCalculationService`
  in `src/ContractorInvoicing.Domain/Services/` — pure in-memory calculation, no
  EF Core dependency. `Recalculate(invoice, isGstRegistered)` fills in each line
  item's `LineSubtotal`/`GstAmount`/`LineTotal` and the invoice's
  `Subtotal`/`GstAmount`/`Total`. GST is 10%, applied only when
  `isGstRegistered` is true *and* the line's `IsTaxable` is true. All money
  values rounded to 2 decimals (`MidpointRounding.AwayFromZero`). Does not touch
  `AmountPaid`/`OutstandingBalance` (payment tracking, a later unit).
- 3 unit tests in `tests/ContractorInvoicing.Tests/Services/InvoiceCalculationServiceTests.cs`,
  matching section 11's three GST/subtotal test cases exactly. Removed the default
  `UnitTest1.cs` scaffold placeholder.
- `dotnet test`: 3 passed, 0 failed. `dotnet build`: 0 warnings, 0 errors.

### Done — 2026-06-29

- Repo initialized (`git init`). 4 commits:
  1. `bcb5b63` — Scaffold solution, projects, and domain entities.
  2. `c3684b2` — Add EF Core DbContext and SQLite persistence.
  3. `bfd8f35` — Add InvoiceCalculationService with unit tests.
  4. `5e3d135` — Add README, CHANGELOG, and project spec.
- `README.md` and `CHANGELOG.md` written at project root.
- `SPEC.md` updated to reflect all decisions and build progress from this
  session (sections 5, 6.1, 6.2, 7, and 14).

### Done — 2026-06-30

- `AbnValidator` added to `src/ContractorInvoicing.Domain/Validation/AbnValidator.cs`.
  Static class, pure function. Validates ABN using the ATO check digit algorithm
  (strip spaces, 11 digits, subtract 1 from first digit, multiply by weights
  [10,1,3,5,7,9,11,13,15,17,19], sum mod 89 == 0). 7 unit tests covering valid
  ABN with/without spaces, wrong check digit, wrong length, non-numeric, null/empty.
- `InvoiceStatusService` added to `src/ContractorInvoicing.Domain/Services/`.
  Interface + implementation. `Apply(invoice, today)` sets `OutstandingBalance`
  (Total − AmountPaid) and derives status: Draft stays Draft; Paid if balance ≤ 0;
  Overdue if due date passed and balance remains; PartiallyPaid if some payment
  exists and balance remains; Sent otherwise. 5 unit tests.
- `Client.NextInvoiceSequence` (int, default 1) added to entity and spec. EF
  migration `AddClientNextInvoiceSequence` generated and applied. Sequence is
  per-client — set to the correct next number when registering a client who already
  has invoices outside the app (e.g. a client with 3 existing invoices → set to 4).
- `InvoiceNumberService` added to `src/ContractorInvoicing.Domain/Services/`.
  Interface + implementation. `Generate(client)` returns `"{BusinessName} Invoice {NNN}"`
  (3-digit zero-padded) using `client.NextInvoiceSequence`, then increments it
  in-memory. Caller is responsible for persisting the updated client. 5 unit tests.
- `dotnet test`: 20 passed, 0 failed. `dotnet build`: 0 warnings, 0 errors.
- SPEC updated: ABN validation requirement added to section 6.1; invoice number
  format updated to `{ClientName} Invoice {NNN}` in section 6.3; `NextInvoiceSequence`
  added to section 7 Client entity; full CSV export on demand added to section 12
  acceptance criteria; CSV invoice seeding import and work log import with AI
  wording step added to section 13 roadmap; section 13.1 work log workflow updated
  with AI suggestion step.

### Done — 2026-07-01

- `PaymentService` added to `src/ContractorInvoicing.Domain/Services/`. Interface +
  implementation. `RecordPayment(invoice, amount, paidAt, method, reference, notes, today)`
  validates amount is positive and does not exceed the outstanding balance, builds
  the `Payment` record, adds `amount` to `invoice.AmountPaid`, then calls
  `InvoiceStatusService.Apply` to recompute status and outstanding balance. First
  service that coordinates two domain services together. Sits in Domain; persistence
  (saving the `Payment` record and updated `Invoice`) is the caller's responsibility.
  94 lines of unit tests in `PaymentServiceTests.cs`.
- Committed as `be2748f`. `dotnet build`: 0 warnings, 0 errors. `dotnet test`:
  27 passed, 0 failed.

### Committed but not yet reflected in this log until now

`InvoiceStatusService` and `Infrastructure.csproj` were still showing as untracked
files in git status as of this session despite matching the "Done — 2026-06-30"
entry above — confirmed their contents match what was already documented, so no
code changes were needed, just noting they should be committed.

### Done — 2026-07-10

First Blazor UI session — went from zero screens to a walkable end-to-end
loop: Profile → Clients → New Invoice → Invoices/CSV export.

- **`/profile`** — Contractor Profile page. Single-row, edit-in-place, ABN
  validated live against `AbnValidator`.
- **`/clients`** — create/list clients. Added `Client.Abn` (nullable,
  optional, validated when provided) mid-session — a real invoice needs the
  client's ABN in the "Bill To" section and the entity was missing it.
  Also added and then removed `Client.Notes` in the same session: added it
  first, then discarded it because nothing in the invoicing flow reads it —
  it's not wired into any invoice generation logic, so it was dead weight.
- **`/invoices/new`** — invoice creation. Each line item has a per-line
  **Hourly / Fixed Price** toggle rather than one generic quantity field —
  added after testing showed a required "Hours" field made no sense for a
  flat project fee. Both modes still map to the same `Quantity`/`UnitPrice`
  the domain layer already expects.
- **`/invoices`** — list page with CSV export actions.
- **`InvoiceCsvService`** (Infrastructure, queries the DB directly) — went
  through two design iterations. First pass: one flat summary row per
  invoice for both bulk and single-invoice export. After comparing output
  against a real reference invoice (PDF/xlsx), this was wrong for the
  single-invoice case — a contractor handing a draft to a client needs
  FROM/BILL TO, line items, totals, and payment details, not a database row.
  Split into two shapes: `ExportAllAsync` stays a flat one-row-per-invoice
  export (the section 12 backup path), `ExportInvoiceAsync` produces a
  structured invoice-document CSV (section 6.3).
- Fixed a regression: `appsettings.json` had lost `ConnectionStrings.Default`
  as an accidental side effect of the `be2748f` commit, causing SQLite to
  silently connect to an empty temp database. Restored.
- Switched `Program.cs` from `AddDbContext` to `AddDbContextFactory` —
  correct pattern for Blazor Server interactive components.
- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 27 passed, 0 failed.
- Added `Assets/` to `.gitignore` — personal reference files (real client
  invoice, ABN, bank details) used for comparison must never be committed.

### What CSV export cannot do — and why PDF is still a separate unit

CSV is a plain-text data format; it cannot carry bold text, table borders,
background colors, or column alignment. The redesigned `ExportInvoiceAsync`
now has the *right content* (matches a real reference invoice field-for-field)
but will never have the *visual polish* of the reference PDF — that gap is
structural to CSV, not a bug to keep testing against. Getting an invoice that
visually looks like a finished document is `InvoicePdfService` (section 10),
a distinct, not-yet-started unit.

### Next session's stated focus

Figure out how to visually edit/format the CSV output from within the
project (as opposed to jumping straight to full PDF generation). Not yet
scoped — needs a spec conversation first: e.g. does "visually edit" mean
generating a styled `.xlsx` instead of `.csv` (a real spreadsheet library
can add borders/bold/color), building a browser-based preview before
download, or something else. Decide the actual mechanism before writing
code.

### Done — 2026-07-13 (not logged at the time)

Committed but not written up in this log until now:

- Payment recording UI (`InvoiceDetail.razor`) — wires the already-built
  `PaymentService` to a page. Click into an invoice, record a payment,
  outstanding balance and status recalculate via `InvoiceStatusService`.
  Answers last session's "Next unit" option 2.
- `InvoiceXlsxService` (Infrastructure) + `/export/invoices/{id}.xlsx`
  endpoint — generates a styled, client-ready invoice workbook (borders,
  bold, alignment) directly from saved invoice data. This is the answer to
  last session's open question ("visually edit the CSV output... styled
  `.xlsx` instead of `.csv`?") — decided in favor of xlsx over a browser
  preview or other mechanism.
- `IInvoiceStatusService`, `IPaymentService`, `IInvoiceXlsxService` registered
  in DI (`Program.cs`).
- Some invoice display fields switched from business name to contact name
  where more appropriate, with matching test updates
  (`InvoiceNumberServiceTests.cs`).

### Spec decisions — 2026-07-15

- **PDF dropped in favor of xlsx.** `InvoicePdfService` is removed from the
  spec (sections 3, 6.3, 10, 11, 12) and replaced with the already-built
  `InvoiceXlsxService`. Reasoning: xlsx solves the same problem (CSV can't
  carry bold/borders/alignment) with a library that's simpler to work with
  in .NET than PDF generation, and it's already shipped and working — no
  reason to build a second export mechanism for the same job. PDF generation
  moved to the out-of-scope list (section 4) rather than deleted outright,
  in case a client-facing PDF is specifically requested later.
- **Seed demo data dropped from v1 scope entirely** (was section 3 and an
  acceptance criterion in section 12). The app will be used live with real
  data from the start rather than staged with fake data for portfolio
  presentation, so there's no seed dataset to build or maintain. Moved to
  the out-of-scope list (section 4).
- **AI features (section 8) stay in v1 scope but are resequenced** — Dashboard
  (section 9) is the next unit; AI assistance comes after. Not a scope cut,
  just build order: dashboard is the app's front door and has no screen yet
  (`/` is still the unmodified Blazor template), so it's a bigger gap in a
  demo-first walkthrough than AI drafting is.

### Known issue, still not fixed — found 2026-07-15

`InvoiceStatusService.Apply()` returns immediately if `invoice.Status ==
InvoiceStatus.Draft` (never re-evaluates it), and no UI action anywhere sets
an invoice to `Sent`. Net effect: an invoice created and left as Draft stays
"Draft" forever in the database even after a payment is recorded against it —
`OutstandingBalance` updates correctly, but `Status` doesn't move to Paid/
PartiallyPaid/Overdue. `Apply()` is also only ever called from
`PaymentService.RecordPayment` — there's no background job or on-load check,
so even a non-Draft invoice's status is a snapshot from the last payment
event, not a live value (e.g. `Sent → Overdue` only updates the next time
money moves against that invoice, not the moment the due date passes).

**Decision (2026-07-15): fix deferred, not bundled with the dashboard unit.**
Dashboard was built to route around the bug instead — see below.

### Done — 2026-07-15

Dashboard (section 9) — first screen after launch. `Dashboard.razor` added at
`/`, replacing the unmodified Blazor template (`Home.razor` deleted, `NavMenu`
label changed from "Home" to "Dashboard"). Same pattern as `Invoices.razor`:
page queries the DB directly via `IDbContextFactory`, no new service layer.

- Total invoiced this month, total paid this month, outstanding balance,
  overdue balance, overdue invoice count, upcoming due (7-day window,
  undocumented in spec — picked a number), recent payments (last 5), client
  income breakdown (Invoiced/Paid/Outstanding per client, all-time), monthly
  income trend (last 6 months paid, plain CSS bar chart, no charting
  library added).
- **Outstanding/Overdue are computed from `OutstandingBalance` and `DueDate`
  directly, not from `Invoice.Status`.** Deliberate: since the Draft-status
  bug above means `Status` can't be trusted yet, the dashboard's numbers are
  correct regardless of whether that bug is ever fixed — only a literal
  `Status` column display (not built) would still read wrong.
- **"Recommended next actions" (a section 9 requirement) was built, then
  removed** at the user's request. First pass computed a deterministic
  read-only list from due dates/balances (the section 6.5 reminder rules) with
  no persistence and no mark-done/ignore. Reason for removal: not scoped as
  part of this unit. Still an open requirement — real version needs
  `AutomationTaskService` (section 6.5) and/or the AI "suggest next actions"
  feature (section 8), neither built yet.
- `dotnet build`: 0 warnings, 0 errors.

### Fixed — 2026-07-15

`.gitignore` was missing the `Assets/` entry despite the 2026-07-10 log entry
above claiming it was added — `Assets/` (real client invoice, ABN, bank
details) was sitting untracked in `git status`, one `git add -A` away from
being committed. Added `Assets/` to `.gitignore` for real this time.

### Next unit (not decided)

Dashboard is done except "Recommended next actions" (deferred, needs
`AutomationTaskService` or AI next-actions first). Two open candidates for
next unit, not yet chosen between:

- Fix the Draft-status bug (add a "Mark as sent" action so invoices can
  leave Draft; the Sent/PartiallyPaid/Overdue/Paid logic in
  `InvoiceStatusService` is already correct and just needs a way to be
  reached).
- AI features (section 8) — already resequenced to come after Dashboard per
  the 2026-07-15 decision above.

