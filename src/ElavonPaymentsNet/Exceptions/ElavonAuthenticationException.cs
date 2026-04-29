using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the API returns a 401 Unauthorised response,
/// indicating invalid or missing integration credentials.
/// </summary>
public sealed class ElavonAuthenticationException : ElavonApiException
{
    public ElavonAuthenticationException(string? rawResponse)
        : base(System.Net.HttpStatusCode.Unauthorized, rawResponse) { }
}
