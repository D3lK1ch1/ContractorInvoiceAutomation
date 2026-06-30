using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Domain.Services;

namespace ContractorInvoicing.Tests.Services;

public class InvoiceStatusServiceTests
{
    private readonly InvoiceStatusService _sut = new();
    private static readonly DateOnly Today = new(2026, 6, 30);

    [Fact]
    public void Apply_SetsPaid_WhenFullyPaid()
    {
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Sent,
            Total = 1000m,
            AmountPaid = 1000m,
            DueDate = Today.AddDays(7)
        };

        _sut.Apply(invoice, Today);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.OutstandingBalance);
    }

    [Fact]
    public void Apply_SetsPartiallyPaid_WhenPartialPaymentMadeAndNotOverdue()
    {
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Sent,
            Total = 1000m,
            AmountPaid = 400m,
            DueDate = Today.AddDays(7)
        };

        _sut.Apply(invoice, Today);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(600m, invoice.OutstandingBalance);
    }

    [Fact]
    public void Apply_SetsOverdue_WhenDueDatePassedAndBalanceRemains()
    {
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Sent,
            Total = 1000m,
            AmountPaid = 0m,
            DueDate = Today.AddDays(-1)
        };

        _sut.Apply(invoice, Today);

        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void Apply_KeepsSent_WhenNoPaymentAndNotOverdue()
    {
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Sent,
            Total = 1000m,
            AmountPaid = 0m,
            DueDate = Today.AddDays(14)
        };

        _sut.Apply(invoice, Today);

        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
        Assert.Equal(1000m, invoice.OutstandingBalance);
    }

    [Fact]
    public void Apply_LeavesDraft_Unchanged()
    {
        var invoice = new Invoice
        {
            Status = InvoiceStatus.Draft,
            Total = 500m,
            AmountPaid = 500m,
            DueDate = Today.AddDays(-30)
        };

        _sut.Apply(invoice, Today);

        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
}
