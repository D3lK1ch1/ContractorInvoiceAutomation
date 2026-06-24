using ContractorInvoicing.Domain.Entities;

namespace ContractorInvoicing.Domain.Services;

public class InvoiceCalculationService : IInvoiceCalculationService
{
    private const decimal GstRate = 0.10m;

    public void Recalculate(Invoice invoice, bool isGstRegistered)
    {
        decimal subtotal = 0;
        decimal gstAmount = 0;

        foreach (var lineItem in invoice.LineItems)
        {
            lineItem.LineSubtotal = Round(lineItem.Quantity * lineItem.UnitPrice);
            lineItem.GstAmount = isGstRegistered && lineItem.IsTaxable
                ? Round(lineItem.LineSubtotal * GstRate)
                : 0;
            lineItem.LineTotal = lineItem.LineSubtotal + lineItem.GstAmount;

            subtotal += lineItem.LineSubtotal;
            gstAmount += lineItem.GstAmount;
        }

        invoice.Subtotal = subtotal;
        invoice.GstAmount = gstAmount;
        invoice.Total = subtotal + gstAmount;
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
