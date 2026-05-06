namespace ElavonPaymentsNet.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Validation.Guard"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class GuardTests
{
    [Fact(DisplayName = "VendorTxCode Blank ThrowsArgumentException")]
    public void VendorTxCode_Blank_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ElavonPaymentsNet.Validation.Guard.VendorTxCode(" ", "vendorTxCode"));

        Assert.Equal("vendorTxCode", ex.ParamName);
    }

    [Fact(DisplayName = "VendorTxCode TooLong ThrowsArgumentException")]
    public void VendorTxCode_TooLong_ThrowsArgumentException()
    {
        var tooLong = new string('A', 41);

        var ex = Assert.Throws<ArgumentException>(() =>
            ElavonPaymentsNet.Validation.Guard.VendorTxCode(tooLong, "vendorTxCode"));

        Assert.Equal("vendorTxCode", ex.ParamName);
        Assert.Contains("cannot exceed 40", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "VendorTxCode InvalidChar ThrowsArgumentException")]
    [InlineData("ORDER 123")]
    [InlineData("ORDER/123")]
    [InlineData("ORDER:123")]
    [InlineData("ORDER@123")]
    public void VendorTxCode_InvalidChar_ThrowsArgumentException(string value)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ElavonPaymentsNet.Validation.Guard.VendorTxCode(value, "vendorTxCode"));

        Assert.Equal("vendorTxCode", ex.ParamName);
        Assert.Contains("invalid characters", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "VendorTxCode BoundaryValid Accepted")]
    [InlineData("A")]
    [InlineData("ORDER-123_ABC.xyz")]
    [InlineData("ORD-20260506-001")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void VendorTxCode_BoundaryValid_Accepted(string value)
    {
        var ex = Record.Exception(() =>
            ElavonPaymentsNet.Validation.Guard.VendorTxCode(value, "vendorTxCode"));

        Assert.Null(ex);
    }
}
