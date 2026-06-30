using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Domain.Services;

namespace ContractorInvoicing.Tests.Services;

public class InvoiceNumberServiceTests
{
    private readonly InvoiceNumberService _sut = new();

    [Fact]
    public void Generate_ReturnsCorrectFormat_WithClientBusinessName()
    {
        var client = new Client { BusinessName = "Enspyr", NextInvoiceSequence = 4 };

        var result = _sut.Generate(client);

        Assert.Equal("Enspyr Invoice 004", result);
    }

    [Fact]
    public void Generate_PadsSequenceToThreeDigits()
    {
        var client = new Client { BusinessName = "Enspyr", NextInvoiceSequence = 1 };

        var result = _sut.Generate(client);

        Assert.Equal("Enspyr Invoice 001", result);
    }

    [Fact]
    public void Generate_HandlesSequenceAboveHundred()
    {
        var client = new Client { BusinessName = "Enspyr", NextInvoiceSequence = 100 };

        var result = _sut.Generate(client);

        Assert.Equal("Enspyr Invoice 100", result);
    }

    [Fact]
    public void Generate_IncrementsSequenceAfterEachCall()
    {
        var client = new Client { BusinessName = "Enspyr", NextInvoiceSequence = 4 };

        _sut.Generate(client);
        var second = _sut.Generate(client);

        Assert.Equal("Enspyr Invoice 005", second);
        Assert.Equal(6, client.NextInvoiceSequence);
    }

    [Fact]
    public void Generate_EachClientHasIndependentSequence()
    {
        var enspyr = new Client { BusinessName = "Enspyr", NextInvoiceSequence = 4 };
        var acme = new Client { BusinessName = "Acme", NextInvoiceSequence = 1 };

        var enspyrNumber = _sut.Generate(enspyr);
        var acmeNumber = _sut.Generate(acme);

        Assert.Equal("Enspyr Invoice 004", enspyrNumber);
        Assert.Equal("Acme Invoice 001", acmeNumber);
    }
}
