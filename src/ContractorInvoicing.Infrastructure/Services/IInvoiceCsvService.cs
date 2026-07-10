namespace ContractorInvoicing.Infrastructure.Services;

public record InvoiceCsvExport(string Csv, string InvoiceNumber);

public interface IInvoiceCsvService
{
    Task<string> ExportAllAsync();
    Task<InvoiceCsvExport?> ExportInvoiceAsync(int invoiceId);
}
