namespace ContractorInvoicing.Infrastructure.Services;

public record InvoiceXlsxExport(byte[] Content, string InvoiceNumber);

public interface IInvoiceXlsxService
{
    Task<InvoiceXlsxExport?> ExportInvoiceAsync(int invoiceId);
}
