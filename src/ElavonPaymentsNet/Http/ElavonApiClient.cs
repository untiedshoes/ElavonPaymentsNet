using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Models.Internal;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Http;

/// <summary>
/// Central HTTP client that handles authentication, serialisation, and error mapping
/// for all Elavon API calls. Not intended for direct use by SDK consumers.
/// </summary>
internal sealed class ElavonApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    internal ElavonApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Sends a request and deserialises the response body into <typeparamref name="TResponse"/>.</summary>
    internal async Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest? payload, string? bearerToken, CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        using var request = BuildRequest(method, path, payload, bearerToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            ThrowApiException(response, body);

        return Deserialise<TResponse>(body, response.StatusCode);
    }

    /// <summary>Sends a request with no request body and deserialises the response.</summary>
    internal async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string path, string? bearerToken, CancellationToken cancellationToken)
        where TResponse : class
    {
        using var request = BuildRequest<object>(method, path, null, bearerToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            ThrowApiException(response, body);

        return Deserialise<TResponse>(body, response.StatusCode);
    }

    /// <summary>Sends a request and does not attempt to deserialise a response body (e.g. 204 No Content).</summary>
    internal async Task SendVoidAsync<TRequest>(HttpMethod method, string path, TRequest? payload, string? bearerToken, CancellationToken cancellationToken)
        where TRequest : class
    {
        using var request = BuildRequest(method, path, payload, bearerToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            ThrowApiException(response, body);
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static HttpRequestMessage BuildRequest<TRequest>(HttpMethod method, string path, TRequest? payload, string? bearerToken)
        where TRequest : class
    {
        var request = new HttpRequestMessage(method, path);

        // Store the bearer token in the property bag so ElavonAuthenticationHandler
        // can apply the correct Authorization header without touching credentials here.
        if (bearerToken is not null)
            request.Options.Set(ElavonRequestContext.BearerTokenKey, bearerToken);

        if (payload is not null)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static TResponse Deserialise<TResponse>(string body, HttpStatusCode statusCode)
        where TResponse : class
    {
        try
        {
            return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
                ?? throw new ElavonApiException(statusCode, body, "EmptyResponse");
        }
        catch (JsonException ex)
        {
            throw new ElavonApiException(statusCode, body, "DeserializationError", ex);
        }
    }

    private static void ThrowApiException(HttpResponseMessage response, string body)
    {
        var statusCode = response.StatusCode;

        // Attempt to parse a machine-readable error code and field errors from the response body.
        // If parsing fails, fall back to raw body only — never swallow the response.
        ApiErrorResponse? error = null;
        string? errorCode = null;
        try
        {
            error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);
            errorCode = error?.Code;
        }
        catch (JsonException) { /* ignore — use raw body only */ }

        IReadOnlyList<ElavonValidationError>? validationErrors = error?.Errors?
            .Select(e => new ElavonValidationError
            {
                Property = e.Property,
                ClientMessage = e.ClientMessage,
                Message = e.Message
            })
            .ToList();

        // Dispatch to the most specific typed exception for this status code.
        // All types inherit ElavonApiException so callers can catch at any level.
        throw statusCode switch
        {
            HttpStatusCode.Unauthorized    => new ElavonAuthenticationException(body),
            HttpStatusCode.BadRequest      => new ElavonValidationException(body, errorCode, validationErrors),
            HttpStatusCode.PaymentRequired => new ElavonPaymentDeclinedException(body, errorCode),
            HttpStatusCode.TooManyRequests => new ElavonRateLimitException(body, errorCode, ParseRetryAfter(response)),
            _ when (int)statusCode >= 500  => new ElavonServerException(statusCode, body, errorCode),
            _                              => new ElavonApiException(statusCode, body, errorCode)
        };
    }

    /// <summary>
    /// Parses the <c>Retry-After</c> response header into a <see cref="TimeSpan"/>,
    /// supporting both delta-seconds (integer) and HTTP-date formats.
    /// Returns <see langword="null"/> if the header is absent or unparseable.
    /// </summary>
    private static TimeSpan? ParseRetryAfter(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var values))
            return null;

        var raw = values.FirstOrDefault();
        if (raw is null)
            return null;

        // Delta-seconds format: "Retry-After: 30"
        if (int.TryParse(raw, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        // HTTP-date format: "Retry-After: Wed, 01 Jan 2025 00:00:00 GMT"
        if (DateTimeOffset.TryParse(raw, out var retryDate))
        {
            var delay = retryDate - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return null;
    }
}
