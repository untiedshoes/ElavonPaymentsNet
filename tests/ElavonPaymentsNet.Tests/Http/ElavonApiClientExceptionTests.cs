using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Services;
using ElavonPaymentsNet.Tests.Http.Fakes;
using System.Net;
using System.Text;

namespace ElavonPaymentsNet.Tests.Http;

/// <summary>
/// Unit tests verifying that <see cref="ElavonApiClient"/> maps each HTTP status code
/// to the correct typed exception via the centralised <c>ThrowApiException</c> method.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ElavonApiClientExceptionTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ElavonTransactionService CreateService(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        var fake = new FakeHttpMessageHandler(handler);
        var httpClient = new HttpClient(fake) { BaseAddress = new Uri("https://sandbox.example.com") };
        var api = new ElavonApiClient(httpClient);
        return new ElavonTransactionService(api);
    }

    private static HttpResponseMessage StatusResponse(HttpStatusCode code, string? body = null) =>
        new(code)
        {
            Content = new StringContent(body ?? string.Empty, Encoding.UTF8, "application/json")
        };

    private static CreateTransactionRequest MinimalRequest() => new()
    {
        TransactionType = TransactionType.Payment,
        VendorTxCode = "TEST-1",
        Amount = 100,
        Currency = "GBP",
        PaymentMethod = new PaymentMethod
        {
            Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
        }
    };

    // -------------------------------------------------------------------------
    // 401 Unauthorised
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that a 401 response throws <see cref="ElavonAuthenticationException"/>.
    /// </summary>
    [Fact]
    public async Task Returns401_ThrowsElavonAuthenticationException()
    {
        var service = CreateService(_ => Task.FromResult(StatusResponse(HttpStatusCode.Unauthorized)));

        var ex = await Assert.ThrowsAsync<ElavonAuthenticationException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(401, ex.HttpStatusCode);
    }

    // -------------------------------------------------------------------------
    // 400 Bad Request
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that a 400 response throws <see cref="ElavonValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Returns400_ThrowsElavonValidationException()
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse(HttpStatusCode.BadRequest, "{\"code\":\"INVALID_FIELD\"}")));

        var ex = await Assert.ThrowsAsync<ElavonValidationException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(400, ex.HttpStatusCode);
        Assert.Equal("INVALID_FIELD", ex.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // 402 Payment Required
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that a 402 response throws <see cref="ElavonPaymentDeclinedException"/>.
    /// </summary>
    [Fact]
    public async Task Returns402_ThrowsElavonPaymentDeclinedException()
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse(HttpStatusCode.PaymentRequired, "{\"code\":\"CARD_DECLINED\"}")));

        var ex = await Assert.ThrowsAsync<ElavonPaymentDeclinedException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(402, ex.HttpStatusCode);
        Assert.Equal("CARD_DECLINED", ex.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // 429 Too Many Requests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that a 429 response throws <see cref="ElavonRateLimitException"/>.
    /// </summary>
    [Fact]
    public async Task Returns429_ThrowsElavonRateLimitException()
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse(HttpStatusCode.TooManyRequests)));

        var ex = await Assert.ThrowsAsync<ElavonRateLimitException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(429, ex.HttpStatusCode);
    }

    /// <summary>
    /// Verifies that a 429 response with a Retry-After delta-seconds header
    /// exposes the parsed <see cref="ElavonRateLimitException.RetryAfter"/> value.
    /// </summary>
    [Fact]
    public async Task Returns429WithRetryAfterHeader_ExposesRetryAfterTimeSpan()
    {
        var service = CreateService(_ =>
        {
            var response = StatusResponse(HttpStatusCode.TooManyRequests);
            response.Headers.Add("Retry-After", "30");
            return Task.FromResult(response);
        });

        var ex = await Assert.ThrowsAsync<ElavonRateLimitException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.NotNull(ex.RetryAfter);
        Assert.Equal(TimeSpan.FromSeconds(30), ex.RetryAfter);
    }

    // -------------------------------------------------------------------------
    // 5xx Server Errors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that a 500 response throws <see cref="ElavonServerException"/>.
    /// </summary>
    [Fact]
    public async Task Returns500_ThrowsElavonServerException()
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse(HttpStatusCode.InternalServerError)));

        var ex = await Assert.ThrowsAsync<ElavonServerException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(500, ex.HttpStatusCode);
    }

    /// <summary>
    /// Verifies that a 503 response also throws <see cref="ElavonServerException"/>.
    /// </summary>
    [Fact]
    public async Task Returns503_ThrowsElavonServerException()
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse(HttpStatusCode.ServiceUnavailable)));

        var ex = await Assert.ThrowsAsync<ElavonServerException>(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.Equal(503, ex.HttpStatusCode);
    }

    // -------------------------------------------------------------------------
    // Exception hierarchy
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that all typed exceptions are catchable as <see cref="ElavonApiException"/>.
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(402)]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(503)]
    public async Task AllTypedExceptions_AreCatchableAsElavonApiException(int statusCode)
    {
        var service = CreateService(_ => Task.FromResult(
            StatusResponse((HttpStatusCode)statusCode)));

        var ex = await Record.ExceptionAsync(
            () => service.CreateTransactionAsync(MinimalRequest()));

        Assert.IsAssignableFrom<ElavonApiException>(ex);
    }
}
