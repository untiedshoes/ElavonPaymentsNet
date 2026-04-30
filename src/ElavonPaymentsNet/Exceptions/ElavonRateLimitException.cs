using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 429 Too Many Requests response.
/// Check <see cref="RetryAfter"/> for the duration to wait before retrying,
/// if the API returned a <c>Retry-After</c> header.
/// </summary>
public sealed class ElavonRateLimitException : ElavonApiException
{
    /// <summary>
    /// The suggested wait duration before retrying, parsed from the
    /// <c>Retry-After</c> response header. <see langword="null"/> if the
    /// header was absent or could not be parsed.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>Initialises a new instance of <see cref="ElavonRateLimitException"/>.</summary>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable error code from the API response, if available.</param>
    /// <param name="retryAfter">Parsed value of the <c>Retry-After</c> response header, if present.</param>
    public ElavonRateLimitException(string? rawResponse, string? errorCode = null, TimeSpan? retryAfter = null)
        : base(System.Net.HttpStatusCode.TooManyRequests, rawResponse, errorCode)
    {
        RetryAfter = retryAfter;
    }
}
