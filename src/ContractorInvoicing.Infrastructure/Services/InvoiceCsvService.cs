using System.Globalization;
using System.Text;
using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorInvoicing.Infrastructure.Services;

public class InvoiceCsvService(IDbContextFactory<ContractorInvoicingDbContext> dbFactory) : IInvoiceCsvService
{
    private static readonly CultureInfo AudCulture = CultureInfo.GetCultureInfo("en-AU");

    private static readonly string[] SummaryHeaders =
    [
        "InvoiceNumber", "Client", "IssueDate", "DueDate", "Status",
        "Subtotal", "GstAmount", "Total", "AmountPaid", "OutstandingBalance"
    ];

    /// <summary>Flat, one-row-per-invoice export for the full-history backup requirement .</summary>
    public async Task<string> ExportAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var invoices = await db.Invoices
            .Include(i => i.Client)
            .OrderBy(i => i.IssueDate)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine(Row(SummaryHeaders));

        foreach (var invoice in invoices)
        {
            sb.AppendLine(Row(
                invoice.InvoiceNumber,
                invoice.Client?.DisplayName ?? "",
                invoice.IssueDate.ToString("yyyy-MM-dd"),
                invoice.DueDate.ToString("yyyy-MM-dd"),
                invoice.Status.ToString(),
                invoice.Subtotal.ToString(CultureInfo.InvariantCulture),
                invoice.GstAmount.ToString(CultureInfo.InvariantCulture),
                invoice.Total.ToString(CultureInfo.InvariantCulture),
                invoice.AmountPaid.ToString(CultureInfo.InvariantCulture),
                invoice.OutstandingBalance.ToString(CultureInfo.InvariantCulture)));
        }

        return sb.ToString();
    }

    /// <summary>Invoice-document-shaped export for a single invoice — a draft you can hand to the client, not a backup row.</summary>
    public async Task<InvoiceCsvExport?> ExportInvoiceAsync(int invoiceId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var invoice = await db.Invoices
            .Include(i => i.Client)
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
            return null;

        var profile = await db.ContractorProfiles.FirstOrDefaultAsync();
        var client = invoice.Client;

        var sb = new StringBuilder();

        sb.AppendLine(Row("INVOICE"));
        sb.AppendLine(Row(profile is { IsGstRegistered: true }
            ? "Tax Invoice"
            : "(not a tax invoice — not registered for GST)"));
        sb.AppendLine();

        sb.AppendLine(Row("FROM", "", "BILL TO"));
        sb.AppendLine(Row(profile?.BusinessName ?? "", "", client?.DisplayName ?? ""));
        sb.AppendLine(Row(
            profile?.Abn is { Length: > 0 } pAbn ? $"ABN {pAbn}" : "",
            "",
            client?.Abn is { Length: > 0 } cAbn ? $"ABN {cAbn}" : ""));
        sb.AppendLine(Row(profile?.Email ?? "", "", client?.Email ?? ""));
        sb.AppendLine();

        sb.AppendLine(Row("Invoice number", invoice.InvoiceNumber));
        sb.AppendLine(Row("Issue date", invoice.IssueDate.ToString("yyyy-MM-dd")));
        if (invoice.PeriodStart is not null && invoice.PeriodEnd is not null)
            sb.AppendLine(Row("Period covered", $"{invoice.PeriodStart:yyyy-MM-dd} - {invoice.PeriodEnd:yyyy-MM-dd}"));
        sb.AppendLine(Row("Due", invoice.Terms ?? invoice.DueDate.ToString("yyyy-MM-dd")));
        sb.AppendLine();

        sb.AppendLine(Row("Description", "Quantity", "Rate (AUD)", "Amount (AUD)"));
        foreach (var line in invoice.LineItems.OrderBy(l => l.SortOrder))
        {
            sb.AppendLine(Row(
                line.Description,
                line.Quantity.ToString(CultureInfo.InvariantCulture),
                line.UnitPrice.ToString("F2", CultureInfo.InvariantCulture),
                line.LineTotal.ToString("F2", CultureInfo.InvariantCulture)));
        }
        sb.AppendLine();

        if (profile is { IsGstRegistered: true })
        {
            sb.AppendLine(Row("Subtotal", "", "", invoice.Subtotal.ToString("C", AudCulture)));
            sb.AppendLine(Row("GST", "", "", invoice.GstAmount.ToString("C", AudCulture)));
            sb.AppendLine(Row("TOTAL", "", "", invoice.Total.ToString("C", AudCulture)));
        }
        else
        {
            sb.AppendLine(Row("TOTAL", "", "", invoice.Total.ToString("C", AudCulture)));
            sb.AppendLine(Row("No GST charged (not registered for GST)."));
        }

        if (!string.IsNullOrWhiteSpace(profile?.PaymentDetails))
        {
            sb.AppendLine();
            sb.AppendLine(Row("PAYMENT DETAILS"));
            sb.AppendLine(Row(profile.PaymentDetails));
        }

        return new InvoiceCsvExport(sb.ToString(), invoice.InvoiceNumber);
    }

    private static string Row(params string[] fields) => string.Join(",", fields.Select(Escape));

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
