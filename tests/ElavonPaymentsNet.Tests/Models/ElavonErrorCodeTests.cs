using ElavonPaymentsNet.Exceptions;

namespace ElavonPaymentsNet.Tests.Models;

[Trait("Category", "Unit")]
public class ElavonErrorCodeTests
{
    [Theory]
    [InlineData(ElavonErrorCode.SystemError,               "2003")]
    [InlineData(ElavonErrorCode.InternalServerError,       "2015")]
    [InlineData(ElavonErrorCode.ScaRequired,               "2022")]
    [InlineData(ElavonErrorCode.PaymentSystemNotSupported, "3069")]
    [InlineData(ElavonErrorCode.BillingStateTooLong,       "3130")]
    [InlineData(ElavonErrorCode.ThreeDsRejectedByIssuer,   "3336")]
    [InlineData(ElavonErrorCode.DuplicateVendorTxCode,     "4001")]
    [InlineData(ElavonErrorCode.AmountOutOfRange,          "4009")]
    [InlineData(ElavonErrorCode.ClientIpRestricted,        "4019")]
    [InlineData(ElavonErrorCode.ThreeDsRulesRequireAuth,   "4026")]
    [InlineData(ElavonErrorCode.ThreeDsCannotAuthoriseCard,"4027")]
    [InlineData(ElavonErrorCode.RefundExceedsOriginal,     "4035")]
    [InlineData(ElavonErrorCode.CardRangeBlocked,          "4043")]
    [InlineData(ElavonErrorCode.AuthoriseExceedsMaximum,   "4044")]
    [InlineData(ElavonErrorCode.TokenNotFound,             "4057")]
    [InlineData(ElavonErrorCode.InvalidPostcodeCharacters, "5055")]
    [InlineData(ElavonErrorCode.UnexpectedCRes,            "5086")]
    [InlineData(ElavonErrorCode.ApplePayInvalidBase64,     "6111")]
    [InlineData(ElavonErrorCode.ApplePayInvalidPayloadFormat, "6112")]
    [InlineData(ElavonErrorCode.ApplePayTavvRequired,      "6116")]
    [InlineData(ElavonErrorCode.ApplePayTokenNotSupplied,  "6149")]
    [InlineData(ElavonErrorCode.ApplePayDomainNotRegistered, "6118")]
    public void ErrorCode_HasExpectedStringValue(string constant, string expected)
    {
        Assert.Equal(expected, constant);
    }

    [Fact(DisplayName = "ErrorCode constants can be used in exception filter pattern")]
    public void ErrorCode_UsableAsExceptionFilter()
    {
        var ex = new ElavonValidationException("raw", ElavonErrorCode.RefundExceedsOriginal);

        Assert.Equal(ElavonErrorCode.RefundExceedsOriginal, ex.ErrorCode);
    }
}
