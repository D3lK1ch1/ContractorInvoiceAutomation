using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public interface IInvoiceStatusService
{
    void Apply(Invoice invoice, DateOnly today);
}
