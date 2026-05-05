using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Services;
using ElavonPaymentsNet.Tests.Http.Fakes;
using System.Net;
using System.Text;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Shared factory helpers used across service unit test classes.
/// </summary>
internal static class ServiceTestHelpers
{
    internal static ElavonTransactionService CreateTransactionService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        string baseAddress = "https://example.com")
        => new(CreateApi(responder, baseAddress));

    internal static ElavonPostPaymentService CreatePostPaymentService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonThreeDsService CreateThreeDsService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonTokensService CreateTokensService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonWalletsService CreateWalletsService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonCardIdentifiersService CreateCardIdentifiersService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonInstructionsService CreateInstructionsService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        => new(CreateApi(responder));

    internal static ElavonApiClient CreateApi(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        string baseAddress = "https://example.com")
    {
        var authHandler = new ElavonAuthenticationHandler("ik", "ip")
        {
            InnerHandler = new FakeHttpMessageHandler(responder)
        };
        var httpClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri(baseAddress)
        };
        return new ElavonApiClient(httpClient);
    }

    internal static string BasicParam()
        => Convert.ToBase64String(Encoding.ASCII.GetBytes("ik:ip"));

    internal static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
        => new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
