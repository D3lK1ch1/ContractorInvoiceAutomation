using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public class PaymentService(IInvoiceStatusService invoiceStatusService) : IPaymentService
{
    public Payment RecordPayment(Invoice invoice, decimal amount, DateOnly paidAt,
                                 string? method, string? reference, string? notes,
                                 DateOnly today)
    {
        if (invoice.Status == InvoiceStatus.Draft)
            throw new ArgumentException("Invoice must be marked as sent before recording a payment.", nameof(invoice));

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        var outstanding = invoice.Total - invoice.AmountPaid;
        if (amount > outstanding)
            throw new ArgumentException("Payment amount exceeds the outstanding balance.", nameof(amount));

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = amount,
            PaidAt = paidAt,
            Method = method,
            Reference = reference,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        invoice.AmountPaid += amount;
        invoiceStatusService.Apply(invoice, today);

        return payment;
    }
}
