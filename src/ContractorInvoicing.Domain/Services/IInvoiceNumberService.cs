using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public interface IInvoiceNumberService
{
    string Generate(Client client);
}
