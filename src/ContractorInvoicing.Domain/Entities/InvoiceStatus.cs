namespace ContractorInvoicing.Domain.Entities;

public enum InvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Overdue
}
