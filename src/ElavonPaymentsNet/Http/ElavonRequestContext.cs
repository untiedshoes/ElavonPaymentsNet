namespace ElavonPaymentsNet.Http;

/// <summary>
/// Keys used to pass per-request context through the <see cref="HttpRequestMessage"/>
/// property bag between <see cref="ElavonApiClient"/> and the handler pipeline.
/// </summary>
internal static class ElavonRequestContext
{
    /// <summary>
    /// Property bag key for an optional Bearer token. When present in
    /// <see cref="HttpRequestMessage.Options"/>, <see cref="ElavonAuthenticationHandler"/>
    /// will use it instead of Basic auth.
    /// </summary>
    internal static readonly HttpRequestOptionsKey<string?> BearerTokenKey =
        new("Elavon-BearerToken");
}
