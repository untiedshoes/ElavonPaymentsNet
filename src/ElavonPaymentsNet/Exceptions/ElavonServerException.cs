using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 5xx Server Error response,
/// indicating a fault on the gateway or upstream infrastructure.
/// </summary>
public sealed class ElavonServerException : ElavonApiException
{
    public ElavonServerException(HttpStatusCode statusCode, string? rawResponse, string? errorCode = null)
        : base(statusCode, rawResponse, errorCode) { }
}
