using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a non-success HTTP status code.
/// </summary>
public class ElavonApiException : Exception
{
    /// <summary>The HTTP status code returned by the API.</summary>
    public int HttpStatusCode { get; }

    /// <summary>The raw response body returned by the API, if available.</summary>
    public string? RawResponse { get; }

    /// <summary>A machine-readable error code from the API response, if available.</summary>
    public string? ErrorCode { get; }

    /// <summary>Initialises a new instance of <see cref="ElavonApiException"/>.</summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable error code from the API response, if available.</param>
    public ElavonApiException(System.Net.HttpStatusCode statusCode, string? rawResponse, string? errorCode = null)
        : base(BuildMessage(statusCode, errorCode))
    {
        HttpStatusCode = (int)statusCode;
        RawResponse = rawResponse;
        ErrorCode = errorCode;
    }

    /// <summary>Initialises a new instance of <see cref="ElavonApiException"/> with an inner exception.</summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable error code from the API response, if available.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ElavonApiException(System.Net.HttpStatusCode statusCode, string? rawResponse, string? errorCode, Exception innerException)
        : base(BuildMessage(statusCode, errorCode), innerException)
    {
        HttpStatusCode = (int)statusCode;
        RawResponse = rawResponse;
        ErrorCode = errorCode;
    }

    private static string BuildMessage(System.Net.HttpStatusCode statusCode, string? errorCode) =>
        errorCode is not null
            ? $"Elavon API returned {(int)statusCode} {statusCode} with error code '{errorCode}'."
            : $"Elavon API returned {(int)statusCode} {statusCode}.";
}
