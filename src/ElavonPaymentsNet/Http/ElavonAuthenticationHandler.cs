using System.Net.Http.Headers;
using System.Text;

namespace ElavonPaymentsNet.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that attaches the correct <c>Authorization</c>
/// header to every outbound Elavon API request.
/// </summary>
/// <remarks>
/// <para>
/// Standard API calls use HTTP <c>Basic</c> authentication: the integration key and
/// password are Base64-encoded and sent on every request.
/// </para>
/// <para>
/// Merchant session key / drop-in flows use HTTP <c>Bearer</c> authentication. The
/// caller signals this by storing the session key in the
/// <see cref="ElavonRequestContext.BearerTokenKey"/> property bag entry on the
/// <see cref="HttpRequestMessage"/>. When present it takes precedence over Basic auth.
/// </para>
/// </remarks>
internal sealed class ElavonAuthenticationHandler : DelegatingHandler
{
    private readonly string _integrationKey;
    private readonly string _integrationPassword;

    internal ElavonAuthenticationHandler(string integrationKey, string integrationPassword)
    {
        _integrationKey = integrationKey;
        _integrationPassword = integrationPassword;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Check for a Bearer token stored in the request property bag by ElavonApiClient.
        // If present, use it; otherwise fall back to Basic auth with the SDK credentials.
        if (request.Options.TryGetValue(ElavonRequestContext.BearerTokenKey, out var bearerToken)
            && !string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        else
        {
            var encoded = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_integrationKey}:{_integrationPassword}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
