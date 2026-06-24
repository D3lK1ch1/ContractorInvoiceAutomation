using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public interface IInvoiceCalculationService
{
    void Recalculate(Invoice invoice, bool isGstRegistered);
}
