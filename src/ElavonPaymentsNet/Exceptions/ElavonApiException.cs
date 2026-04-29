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

    public ElavonApiException(System.Net.HttpStatusCode statusCode, string? rawResponse, string? errorCode = null)
        : base(BuildMessage(statusCode, errorCode))
    {
        HttpStatusCode = (int)statusCode;
        RawResponse = rawResponse;
        ErrorCode = errorCode;
    }

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
