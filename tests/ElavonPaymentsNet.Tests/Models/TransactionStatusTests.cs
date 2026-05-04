using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Tests.Models;

[Trait("Category", "Unit")]
public class TransactionStatusTests
{
    [Theory]
    [InlineData("Ok", TransactionStatusKind.Ok)]
    [InlineData("NotAuthed", TransactionStatusKind.NotAuthed)]
    [InlineData("Rejected", TransactionStatusKind.Rejected)]
    [InlineData("Malformed", TransactionStatusKind.Malformed)]
    [InlineData("Invalid", TransactionStatusKind.Invalid)]
    [InlineData("Error", TransactionStatusKind.Error)]
    [InlineData("Registered", TransactionStatusKind.Registered)]
    [InlineData("Pending", TransactionStatusKind.Pending)]
    [InlineData("3DAuth", TransactionStatusKind.ThreeDAuth)]
    [InlineData("ok", TransactionStatusKind.Ok)]
    [InlineData("3dauth", TransactionStatusKind.ThreeDAuth)]
    [InlineData("something-new-from-api", TransactionStatusKind.Unknown)]
    [InlineData("", TransactionStatusKind.Unknown)]
    public void ParseKind_MapsExpectedStatus(string status, TransactionStatusKind expected)
    {
        var actual = TransactionStatus.ParseKind(status);

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ParseKind Null ReturnsUnknown")]
    public void ParseKind_Null_ReturnsUnknown()
    {
        var actual = TransactionStatus.ParseKind(null);

        Assert.Equal(TransactionStatusKind.Unknown, actual);
    }

    [Fact(DisplayName = "PaymentResponse StatusKind ReflectsStatus")]
    public void PaymentResponse_StatusKind_ReflectsStatus()
    {
        var response = new PaymentResponse { Status = "3DAuth" };

        Assert.Equal(TransactionStatusKind.ThreeDAuth, response.StatusKind);
    }

    [Fact(DisplayName = "Complete3DsResponse StatusKind ReflectsStatus")]
    public void Complete3DsResponse_StatusKind_ReflectsStatus()
    {
        var response = new Complete3DsResponse { Status = "Ok" };

        Assert.Equal(TransactionStatusKind.Ok, response.StatusKind);
    }

    [Fact(DisplayName = "PostPaymentResponse StatusKind UnknownForUnexpected")]
    public void PostPaymentResponse_StatusKind_UnknownForUnexpected()
    {
        var response = new PostPaymentResponse { Status = "NewFutureStatus" };

        Assert.Equal(TransactionStatusKind.Unknown, response.StatusKind);
    }
}
