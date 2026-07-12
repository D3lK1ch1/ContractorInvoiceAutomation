using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public class InvoiceNumberService : IInvoiceNumberService
{
    public string Generate(Client client)
    {
        var number = $"{client.DisplayName} Invoice {client.NextInvoiceSequence:D3}";
        client.NextInvoiceSequence++;
        return number;
    }
}
