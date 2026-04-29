using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Models.Internal;
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
    internal async Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest? payload, string? bearerToken, string integrationKey, string integrationPassword, CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        using var request = BuildRequest(method, path, payload, bearerToken, integrationKey, integrationPassword);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            ThrowApiException(response.StatusCode, body);

        return Deserialise<TResponse>(body, response.StatusCode);
    }

    /// <summary>Sends a request with no request body and deserialises the response.</summary>
    internal async Task<TResponse> SendAsync<TResponse>(HttpMethod method,string path,string? bearerToken,string integrationKey,string integrationPassword,CancellationToken cancellationToken)
        where TResponse : class
    {
        using var request = BuildRequest<object>(method, path, null, bearerToken, integrationKey, integrationPassword);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            ThrowApiException(response.StatusCode, body);

        return Deserialise<TResponse>(body, response.StatusCode);
    }

    /// <summary>Sends a request and does not attempt to deserialise a response body (e.g. 204 No Content).</summary>
    internal async Task SendVoidAsync<TRequest>(HttpMethod method, string path, TRequest? payload, string? bearerToken, string integrationKey, string integrationPassword, CancellationToken cancellationToken)
        where TRequest : class
    {
        using var request = BuildRequest(method, path, payload, bearerToken, integrationKey, integrationPassword);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            ThrowApiException(response.StatusCode, body);
        }
    }

    /// <summary>Sends a request with an empty body (e.g. void) and deserialises the response.</summary>
    internal async Task<TResponse> SendEmptyAsync<TResponse>(HttpMethod method,string path,string integrationKey,string integrationPassword,CancellationToken cancellationToken)
        where TResponse : class
    {
        using var request = BuildRequest<object>(method, path, null, null, integrationKey, integrationPassword);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            ThrowApiException(response.StatusCode, body);

        return Deserialise<TResponse>(body, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static HttpRequestMessage BuildRequest<TRequest>(HttpMethod method,string path,TRequest? payload,string? bearerToken,string integrationKey,string integrationPassword)
        where TRequest : class
    {
        var request = new HttpRequestMessage(method, path);

        if (bearerToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        else
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{integrationKey}:{integrationPassword}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

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

    private static void ThrowApiException(HttpStatusCode statusCode, string body)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
            throw new ElavonAuthenticationException(body);

        string? errorCode = null;
        try
        {
            var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);
            errorCode = error?.Code;
        }
        catch (JsonException) { /* ignore — use raw body only */ }

        throw new ElavonApiException(statusCode, body, errorCode);
    }
}
