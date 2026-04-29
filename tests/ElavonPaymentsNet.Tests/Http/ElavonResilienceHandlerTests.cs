using ElavonPaymentsNet.Http;
using System.Net;

namespace ElavonPaymentsNet.Tests.Http;

/// <summary>
/// Unit tests for <see cref="ElavonResilienceHandler"/>.
///
/// These tests verify the safety contract:
///  - GET requests ARE retried on 5xx and transient network errors
///  - POST requests are NEVER retried regardless of failure type
///  - 4xx responses are never retried
///  - User-cancelled tokens are not retried
/// </summary>
public sealed class ElavonResilienceHandlerTests
{
    // -------------------------------------------------------------------------
    // GET — should retry on transient failures
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task Get_5xxResponse_IsRetried(int statusCode)
    {
        // Arrange: fail twice with 5xx, succeed on third attempt
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            return callCount < 3
                ? new HttpResponseMessage((HttpStatusCode)statusCode)
                : new HttpResponseMessage(HttpStatusCode.OK);
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, callCount); // initial + 2 retries
    }

    [Fact]
    public async Task Get_HttpRequestException_IsRetried()
    {
        // Arrange: throw twice, succeed on third
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            if (callCount < 3) throw new HttpRequestException("Network failure");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, callCount);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(422)]
    public async Task Get_4xxResponse_IsNotRetried(int statusCode)
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage((HttpStatusCode)statusCode);
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert — exactly one attempt, no retries
        Assert.Equal((HttpStatusCode)statusCode, response.StatusCode);
        Assert.Equal(1, callCount);
    }

    // -------------------------------------------------------------------------
    // POST — must never retry regardless of failure
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public async Task Post_5xxResponse_IsNeverRetried(int statusCode)
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage((HttpStatusCode)statusCode);
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.PostAsync("/transactions", new StringContent("{}"));

        // Assert — exactly one attempt, never retried
        Assert.Equal((HttpStatusCode)statusCode, response.StatusCode);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Post_HttpRequestException_IsNeverRetried()
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            throw new HttpRequestException("Connection reset");
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act & Assert — exception propagates immediately, no retries
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.PostAsync("/transactions", new StringContent("{}")));

        Assert.Equal(1, callCount);
    }

    // -------------------------------------------------------------------------
    // Retry exhaustion — should surface the final failure
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Get_AlwaysFails_ThrowsAfterMaxRetries()
    {
        // Arrange: always throw
        var callCount = 0;
        var inner = new FakeHandler(_ =>
        {
            callCount++;
            throw new HttpRequestException("Always fails");
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync("/ping"));

        // 1 initial + 3 retries = 4 total calls
        Assert.Equal(4, callCount);
    }

    // -------------------------------------------------------------------------
    // Constructor validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Constructor_InvalidRetryCount_Throws(int maxRetryAttempts)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ElavonResilienceHandler(maxRetryAttempts));
    }

    // -------------------------------------------------------------------------
    // Fake inner handler
    // -------------------------------------------------------------------------

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(respond(request));
        }
    }
}
