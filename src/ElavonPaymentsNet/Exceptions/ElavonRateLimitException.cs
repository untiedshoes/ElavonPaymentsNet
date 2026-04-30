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

    public ElavonRateLimitException(string? rawResponse, string? errorCode = null, TimeSpan? retryAfter = null)
        : base(System.Net.HttpStatusCode.TooManyRequests, rawResponse, errorCode)
    {
        RetryAfter = retryAfter;
    }
}
