using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public class InvoiceStatusService : IInvoiceStatusService
{
    public void Apply(Invoice invoice, DateOnly today)
    {
        invoice.OutstandingBalance = invoice.Total - invoice.AmountPaid;

        if (invoice.Status == InvoiceStatus.Draft)
            return;

        if (invoice.OutstandingBalance <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
            return;
        }

        if (invoice.DueDate < today)
        {
            invoice.Status = InvoiceStatus.Overdue;
            return;
        }

        if (invoice.AmountPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
            return;
        }

        invoice.Status = InvoiceStatus.Sent;
    }
}
