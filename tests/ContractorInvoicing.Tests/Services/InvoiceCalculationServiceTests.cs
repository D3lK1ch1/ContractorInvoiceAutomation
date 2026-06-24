using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Domain.Services;

namespace ContractorInvoicing.Tests.Services;

public class InvoiceCalculationServiceTests
{
    private readonly InvoiceCalculationService _sut = new();

    [Fact]
    public void Recalculate_SumsLineItemSubtotals_RegardlessOfGstRegistration()
    {
        var invoice = new Invoice
        {
            LineItems =
            [
                new InvoiceLineItem { Quantity = 4, UnitPrice = 24.95m, IsTaxable = true },
                new InvoiceLineItem { Quantity = 2, UnitPrice = 100m, IsTaxable = false }
            ]
        };

        _sut.Recalculate(invoice, isGstRegistered: false);

        Assert.Equal(299.80m, invoice.Subtotal);
    }

    [Fact]
    public void Recalculate_AppliesTenPercentGst_OnlyToTaxableLines_WhenGstRegistered()
    {
        var invoice = new Invoice
        {
            LineItems =
            [
                new InvoiceLineItem { Quantity = 1, UnitPrice = 100m, IsTaxable = true },
                new InvoiceLineItem { Quantity = 1, UnitPrice = 100m, IsTaxable = false }
            ]
        };

        _sut.Recalculate(invoice, isGstRegistered: true);

        var taxableLine = invoice.LineItems[0];
        var nonTaxableLine = invoice.LineItems[1];

        Assert.Equal(10m, taxableLine.GstAmount);
        Assert.Equal(110m, taxableLine.LineTotal);
        Assert.Equal(0m, nonTaxableLine.GstAmount);
        Assert.Equal(100m, nonTaxableLine.LineTotal);

        Assert.Equal(200m, invoice.Subtotal);
        Assert.Equal(10m, invoice.GstAmount);
        Assert.Equal(210m, invoice.Total);
    }

    [Fact]
    public void Recalculate_SetsZeroGstAndTotalEqualsSubtotal_WhenNotGstRegistered()
    {
        var invoice = new Invoice
        {
            LineItems =
            [
                new InvoiceLineItem { Quantity = 1, UnitPrice = 100m, IsTaxable = true },
                new InvoiceLineItem { Quantity = 1, UnitPrice = 50m, IsTaxable = false }
            ]
        };

        _sut.Recalculate(invoice, isGstRegistered: false);

        Assert.All(invoice.LineItems, li => Assert.Equal(0m, li.GstAmount));
        Assert.Equal(0m, invoice.GstAmount);
        Assert.Equal(invoice.Subtotal, invoice.Total);
    }
}
