using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 400 Bad Request response,
/// indicating that one or more request fields failed validation.
/// Inspect <see cref="ElavonApiException.RawResponse"/> for field-level detail.
/// </summary>
public sealed class ElavonValidationException : ElavonApiException
{
    public ElavonValidationException(string? rawResponse, string? errorCode = null)
        : base(System.Net.HttpStatusCode.BadRequest, rawResponse, errorCode) { }
}
