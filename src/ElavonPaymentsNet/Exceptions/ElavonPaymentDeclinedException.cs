using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 402 Payment Required response,
/// indicating that the card or payment was declined by the issuer or gateway.
/// This is a business outcome, not an API or infrastructure error.
/// </summary>
public sealed class ElavonPaymentDeclinedException : ElavonApiException
{
    /// <summary>Initialises a new instance of <see cref="ElavonPaymentDeclinedException"/>.</summary>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable decline code from the API response, if available.</param>
    public ElavonPaymentDeclinedException(string? rawResponse, string? errorCode = null)
        : base(System.Net.HttpStatusCode.PaymentRequired, rawResponse, errorCode) { }
}
