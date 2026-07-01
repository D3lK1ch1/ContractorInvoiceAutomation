using ContractorInvoicing.Domain.Entities;
using ContractorInvoicing.Domain.Services;

namespace ContractorInvoicing.Tests.Services;

public class InvoiceNumberServiceTests
{
    private readonly InvoiceNumberService _sut = new();

    [Fact]
    public void Generate_ReturnsCorrectFormat_WithClientBusinessName()
    {
        var client = new Client { BusinessName = "Acme", NextInvoiceSequence = 4 };

        var result = _sut.Generate(client);

        Assert.Equal("Acme Invoice 004", result);
    }

    [Fact]
    public void Generate_PadsSequenceToThreeDigits()
    {
        var client = new Client { BusinessName = "Acme", NextInvoiceSequence = 1 };

        var result = _sut.Generate(client);

        Assert.Equal("Acme Invoice 001", result);
    }

    [Fact]
    public void Generate_HandlesSequenceAboveHundred()
    {
        var client = new Client { BusinessName = "Acme", NextInvoiceSequence = 100 };

        var result = _sut.Generate(client);

        Assert.Equal("Acme Invoice 100", result);
    }

    [Fact]
    public void Generate_IncrementsSequenceAfterEachCall()
    {
        var client = new Client { BusinessName = "Acme", NextInvoiceSequence = 4 };

        _sut.Generate(client);
        var second = _sut.Generate(client);

        Assert.Equal("Acme Invoice 005", second);
        Assert.Equal(6, client.NextInvoiceSequence);
    }

    [Fact]
    public void Generate_EachClientHasIndependentSequence()
    {
        var acme = new Client { BusinessName = "Acme", NextInvoiceSequence = 4 };
        var globex = new Client { BusinessName = "Globex", NextInvoiceSequence = 1 };

        var acmeNumber = _sut.Generate(acme);
        var globexNumber = _sut.Generate(globex);

        Assert.Equal("Acme Invoice 004", acmeNumber);
        Assert.Equal("Globex Invoice 001", globexNumber);
    }
}
