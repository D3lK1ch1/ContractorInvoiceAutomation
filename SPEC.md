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
- PDF invoice generation.
- CSV export of invoice data (for archiving in a Google Sheets tab or similar — no live API integration in v1).
- Dashboard for totals, overdue invoices, and next actions.
- Practical AI assistance for wording, follow-up drafts, and anomaly checks.
- Seed demo data for portfolio presentation.

## 4. Out of Scope for V1

The following should be deliberately excluded from v1 to keep the first version focused:

- Multi-tenant SaaS accounts.
- Stripe or card payment collection.
- Automatic email sending.
- Bank feed integration.
- Live Google Sheets / Drive API write-back (v1 covers CSV export only; the user imports/pastes it into Sheets manually).
- Full accounting ledger.
- BAS/tax filing features.
- OCR extraction from uploaded invoices.
- Multi-country tax compliance.
- Mobile app.
- Role-based team permissions.

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

### 6.2 Client Management

The user can create, edit, and archive clients.

Client fields:

- Client contact name.
- Client business name.
- Email.
- Notes.
- Default payment terms override.
- Default hourly rate (optional — pre-fills new line items for this client; a specific invoice can still override it, e.g. a project with a different agreed rate).

### 6.3 Invoice Creation

The user can create an invoice from a client.

Invoice behaviour:

- Auto-generate invoice number using a simple sequence.
- Default issue date to today.
- Default due date from payment terms.
- Optionally set period covered (start/end date of the billing period).
- Add line items, pre-filling rate from the client's default hourly rate when set.
- Calculate totals immediately.
- Save as draft.
- Mark as sent.
- Export PDF.
- Export CSV (spreadsheet-compatible, for archiving in Google Sheets or similar).

Line item types:

- Hourly work: description, hours, rate.
- Fixed-price work: description, quantity, unit price.
- Mixed line items should be supported by the same line item model.

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
- Notes (nullable)
- DefaultPaymentTermsDays
- DefaultHourlyRate (nullable)
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
- A .NET PDF generation library with suitable licensing.
- Built-in dependency injection.
- xUnit or NUnit for tests.
- Playwright or bUnit for selected UI tests if the UI grows.

Architecture:

- Modular monolith.
- Keep domain logic separate from UI components.
- Keep invoice calculation logic testable without the database.
- Keep AI integration behind an interface.
- Keep PDF generation behind a service interface.

Suggested service boundaries:

- InvoiceCalculationService.
- InvoiceNumberService.
- InvoiceStatusService.
- PaymentService.
- AutomationTaskService.
- InvoicePdfService.
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
- Generate invoice PDF from saved invoice data.
- Generate invoice CSV export from saved invoice data.

UI acceptance scenarios:

- User creates contractor profile.
- User creates client.
- User creates invoice.
- User exports invoice PDF.
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
- The app can export a professional PDF invoice.
- The app can export invoice data as CSV for spreadsheet archiving.
- The user can record one or more payments.
- Invoice statuses update correctly.
- The dashboard reflects invoice and payment state.
- The app generates follow-up task recommendations.
- AI can draft invoice wording and payment reminders through a replaceable service.
- Demo data is available without exposing real personal or client information.
- Core business logic has automated tests.

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
- The user reviews and can edit the generated line items before saving the invoice — import only prepares a draft, it never creates or sends an invoice on its own.

## 14. Build Progress

Status as of 2026-06-10.

### Done

- Solution scaffolded: `ContractorInvoicing.sln` with 4 projects.
  - `src/ContractorInvoicing.Domain` — class library, no dependencies.
  - `src/ContractorInvoicing.Infrastructure` — class library, references Domain.
  - `src/ContractorInvoicing.Web` — Blazor Web App (Server interactivity), references Domain + Infrastructure.
  - `tests/ContractorInvoicing.Tests` — xUnit, references Domain.
- Core entities added under `src/ContractorInvoicing.Domain/Entities/`, matching section 7 field-for-field:
  - `ContractorProfile`, `Client`, `Invoice`, `InvoiceLineItem`, `Payment`, `AutomationTask`.
  - Enums: `InvoiceStatus`, `AutomationTaskType`, `AutomationTaskStatus`.
- `.gitignore` added (bin/obj/SQLite files). Repo is **not yet a git repository** — needs `git init` before commits.
- `dotnet build` succeeds, 0 warnings, 0 errors.
- No tests yet — entities are plain POCOs with no behavior, so nothing meaningful to test until calculation/status logic exists.

### Open judgment calls to confirm later

- `ContractorProfile.Country` / `Client.Country` default to `"Australia"`.
- `Client.DefaultPaymentTermsDays` is `int?` (override, falls back to contractor default when null); `ContractorProfile.DefaultPaymentTermsDays` is `int` defaulting to 14.
- Dates use `DateOnly` (IssueDate, DueDate, PaidAt, ScheduledFor); timestamps use `DateTime` (CreatedAt/UpdatedAt).

### Spec decisions — 2026-06-24

Reviewed two real reference files (an actual invoice sent to client Enspyr, and
a freeform work-log document) against this spec, and checked three GitHub repos
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

### Next unit (not started)

- `InvoiceNumberService` and/or `InvoiceStatusService` (sequence generation,
  Draft/Sent/Partially Paid/Paid/Overdue transitions per section 6.4) — pick
  whichever Delia wants to tackle first when resuming.

