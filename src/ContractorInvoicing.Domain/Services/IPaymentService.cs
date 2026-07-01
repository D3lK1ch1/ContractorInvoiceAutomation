using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public interface IPaymentService
{
    Payment RecordPayment(Invoice invoice, decimal amount, DateOnly paidAt,
                          string? method, string? reference, string? notes,
                          DateOnly today);
}
