using ContractorInvoicing.Domain.Validation;

namespace ContractorInvoicing.Tests.Validation;

public class AbnValidatorTests
{
    [Fact]
    public void IsValid_ReturnsTrue_ForKnownValidAbn()
    {
        // ATO's own ABN: 51 824 753 556
        Assert.True(AbnValidator.IsValid("51 824 753 556"));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidAbnWithoutSpaces()
    {
        Assert.True(AbnValidator.IsValid("51824753556"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidCheckDigit()
    {
        // Last digit changed from 6 to 7
        Assert.False(AbnValidator.IsValid("51 824 753 557"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFewerThanElevenDigits()
    {
        Assert.False(AbnValidator.IsValid("5182475355"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenMoreThanElevenDigits()
    {
        Assert.False(AbnValidator.IsValid("518247535560"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenNonNumericCharactersPresent()
    {
        Assert.False(AbnValidator.IsValid("5182475355A"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNullOrEmpty()
    {
        Assert.False(AbnValidator.IsValid(null!));
        Assert.False(AbnValidator.IsValid(""));
        Assert.False(AbnValidator.IsValid("   "));
    }
}
