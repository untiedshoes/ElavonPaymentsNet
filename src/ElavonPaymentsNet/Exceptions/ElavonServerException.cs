using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 5xx Server Error response,
/// indicating a fault on the gateway or upstream infrastructure.
/// </summary>
public sealed class ElavonServerException : ElavonApiException
{
    /// <summary>Initialises a new instance of <see cref="ElavonServerException"/>.</summary>
    /// <param name="statusCode">The 5xx HTTP status code returned by the gateway.</param>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable error code from the API response, if available.</param>
    public ElavonServerException(HttpStatusCode statusCode, string? rawResponse, string? errorCode = null)
        : base(statusCode, rawResponse, errorCode) { }
}
