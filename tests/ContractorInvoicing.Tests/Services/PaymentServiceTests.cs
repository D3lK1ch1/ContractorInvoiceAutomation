using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Domain.Services;

namespace ContractorInvoicing.Tests.Services;

public class PaymentServiceTests
{
    private static readonly DateOnly Today = new(2026, 7, 1);
    private readonly PaymentService _sut = new(new InvoiceStatusService());

    private static Invoice SentInvoice(decimal total) => new()
    {
        Status = InvoiceStatus.Sent,
        Total = total,
        AmountPaid = 0,
        DueDate = Today.AddDays(14)
    };

    [Fact]
    public void RecordPayment_FullPayment_SetsStatusPaid()
    {
        var invoice = SentInvoice(1000m);

        _sut.RecordPayment(invoice, 1000m, Today, null, null, null, Today);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.OutstandingBalance);
    }

    [Fact]
    public void RecordPayment_PartialPayment_SetsStatusPartiallyPaid()
    {
        var invoice = SentInvoice(1000m);

        _sut.RecordPayment(invoice, 400m, Today, null, null, null, Today);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(600m, invoice.OutstandingBalance);
        Assert.Equal(400m, invoice.AmountPaid);
    }

    [Fact]
    public void RecordPayment_TwoPartialPayments_SetsStatusPaidWhenBalanceCleared()
    {
        var invoice = SentInvoice(1000m);

        _sut.RecordPayment(invoice, 600m, Today, null, null, null, Today);
        _sut.RecordPayment(invoice, 400m, Today, null, null, null, Today);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.OutstandingBalance);
    }

    [Fact]
    public void RecordPayment_DraftInvoice_Throws()
    {
        var invoice = SentInvoice(1000m);
        invoice.Status = InvoiceStatus.Draft;

        Assert.Throws<ArgumentException>(() =>
            _sut.RecordPayment(invoice, 500m, Today, null, null, null, Today));
    }

    [Fact]
    public void RecordPayment_ZeroAmount_Throws()
    {
        var invoice = SentInvoice(1000m);

        Assert.Throws<ArgumentException>(() =>
            _sut.RecordPayment(invoice, 0m, Today, null, null, null, Today));
    }

    [Fact]
    public void RecordPayment_NegativeAmount_Throws()
    {
        var invoice = SentInvoice(1000m);

        Assert.Throws<ArgumentException>(() =>
            _sut.RecordPayment(invoice, -50m, Today, null, null, null, Today));
    }

    [Fact]
    public void RecordPayment_ExceedsOutstandingBalance_Throws()
    {
        var invoice = SentInvoice(1000m);

        Assert.Throws<ArgumentException>(() =>
            _sut.RecordPayment(invoice, 1001m, Today, null, null, null, Today));
    }

    [Fact]
    public void RecordPayment_ReturnsPaymentWithCorrectFields()
    {
        var invoice = SentInvoice(1000m);
        var paidAt = new DateOnly(2026, 7, 1);

        var payment = _sut.RecordPayment(invoice, 500m, paidAt, "Bank Transfer", "REF123", "First payment", Today);

        Assert.Equal(500m, payment.Amount);
        Assert.Equal(paidAt, payment.PaidAt);
        Assert.Equal("Bank Transfer", payment.Method);
        Assert.Equal("REF123", payment.Reference);
    }
}
