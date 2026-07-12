using ClosedXML.Excel;
using ContractorInvoicing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractorInvoicing.Infrastructure.Services;

/// <summary>Formatted single-invoice export matching the reference PDF layout (header blocks + line-item table + total).</summary>
public class InvoiceXlsxService(IDbContextFactory<ContractorInvoicingDbContext> dbFactory) : IInvoiceXlsxService
{
    private static readonly XLColor TableHeaderFill = XLColor.FromArgb(0x1F, 0x29, 0x37);
    private static readonly XLColor TotalRowFill = XLColor.FromArgb(0xDC, 0xE6, 0xF1);

    public async Task<InvoiceXlsxExport?> ExportInvoiceAsync(int invoiceId)
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

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Invoice");

        ws.Column(1).Width = 32;
        ws.Column(2).Width = 10;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 14;

        ws.Cell(1, 1).Value = "INVOICE";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 18;

        ws.Cell(2, 1).Value = profile is { IsGstRegistered: true }
            ? "Tax Invoice"
            : "(not a tax invoice — not registered for GST)";
        ws.Cell(2, 1).Style.Font.Italic = true;
        ws.Cell(2, 1).Style.Font.FontSize = 9;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromArgb(0x66, 0x66, 0x66);

        ws.Cell(4, 1).Value = "FROM";
        ws.Cell(4, 3).Value = "BILL TO";
        ws.Range(4, 1, 4, 3).Cells().Style.Font.Bold = true;

        ws.Cell(5, 1).Value = profile?.BusinessName ?? "";
        ws.Cell(5, 3).Value = client?.DisplayName ?? "";

        ws.Cell(6, 1).Value = profile?.Abn is { Length: > 0 } pAbn ? $"ABN {pAbn}" : "";
        ws.Cell(6, 3).Value = client?.Abn is { Length: > 0 } cAbn ? $"ABN {cAbn}" : "";

        ws.Cell(7, 1).Value = profile?.Email ?? "";
        ws.Cell(7, 3).Value = client?.Email ?? "";

        var metaRow = 9;
        WriteMetaRow(ws, metaRow++, "Invoice number", invoice.InvoiceNumber);
        WriteMetaRow(ws, metaRow++, "Issue date", invoice.IssueDate.ToString("yyyy-MM-dd"));
        if (invoice.PeriodStart is not null && invoice.PeriodEnd is not null)
            WriteMetaRow(ws, metaRow++, "Period covered", $"{invoice.PeriodStart:yyyy-MM-dd} - {invoice.PeriodEnd:yyyy-MM-dd}");
        WriteMetaRow(ws, metaRow++, "Due", invoice.Terms ?? invoice.DueDate.ToString("yyyy-MM-dd"));

        var tableHeaderRow = metaRow + 1;
        string[] headers = ["Description", "Hours", "Rate (AUD)", "Amount (AUD)"];
        for (var col = 0; col < headers.Length; col++)
        {
            var cell = ws.Cell(tableHeaderRow, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = TableHeaderFill;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var row = tableHeaderRow + 1;
        foreach (var line in invoice.LineItems.OrderBy(l => l.SortOrder))
        {
            ws.Cell(row, 1).Value = line.Description;
            ws.Cell(row, 2).Value = line.Quantity;
            ws.Cell(row, 3).Value = line.UnitPrice;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 4).Value = line.LineTotal;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00";

            ws.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        var totalRow = row;
        ws.Range(totalRow, 1, totalRow, 3).Merge();
        ws.Cell(totalRow, 1).Value = "TOTAL";
        ws.Cell(totalRow, 1).Style.Font.Bold = true;
        ws.Cell(totalRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        ws.Cell(totalRow, 4).Value = invoice.Total;
        ws.Cell(totalRow, 4).Style.NumberFormat.Format = "0.00";
        ws.Cell(totalRow, 4).Style.Font.Bold = true;

        ws.Range(totalRow, 1, totalRow, 4).Style.Fill.BackgroundColor = TotalRowFill;
        ws.Range(totalRow, 1, totalRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        if (!string.IsNullOrWhiteSpace(profile?.PaymentDetails))
        {
            var labelRow = totalRow + 2;
            ws.Cell(labelRow, 1).Value = "PAYMENT DETAILS";
            ws.Cell(labelRow, 1).Style.Font.Bold = true;

            var detailsRow = labelRow + 1;
            foreach (var line in profile.PaymentDetails.Replace("\r\n", "\n").Split('\n'))
            {
                ws.Range(detailsRow, 1, detailsRow, 4).Merge();
                ws.Cell(detailsRow, 1).Value = line;
                detailsRow++;
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new InvoiceXlsxExport(stream.ToArray(), invoice.InvoiceNumber);
    }

    private static void WriteMetaRow(IXLWorksheet ws, int row, string label, string value)
    {
        ws.Cell(row, 3).Value = label;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(row, 4).Value = value;
    }
}
