using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElavonPaymentsNet.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that logs outbound requests and inbound responses
/// at the HTTP infrastructure level.
/// </summary>
/// <remarks>
/// Logging is intentionally minimal and safe for production:
/// <list type="bullet">
///   <item>No credentials, card data, or request/response bodies are logged.</item>
///   <item>When no <see cref="ILogger"/> is supplied, a <see cref="NullLogger"/> is used
///   and this handler has zero overhead.</item>
/// </list>
/// </remarks>
internal sealed class ElavonLoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    internal ElavonLoggingHandler(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<ElavonLoggingHandler>() ?? NullLogger<ElavonLoggingHandler>.Instance;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Elavon SDK → {Method} {Uri}",
            request.Method,
            request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "Elavon SDK ← {StatusCode} {ReasonPhrase} ({Method} {Uri})",
            (int)response.StatusCode,
            response.ReasonPhrase,
            request.Method,
            request.RequestUri);

        return response;
    }
}
