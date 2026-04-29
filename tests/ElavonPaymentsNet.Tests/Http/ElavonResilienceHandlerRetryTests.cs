using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Tests.Http.Fakes;
using System.Net;

namespace ElavonPaymentsNet.Tests.Http;

/// <summary>
/// Unit tests focused on retry behavior for <see cref="ElavonResilienceHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ElavonResilienceHandlerRetryTests
{
    /// <summary>
    /// Verifies that GET requests are retried on 5xx responses and eventually succeed.
    /// </summary>
    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task Get_5xxResponse_IsRetried(int statusCode)
    {
        // Arrange: fail twice with 5xx, succeed on third attempt.
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return Task.FromResult(callCount < 3
                ? new HttpResponseMessage((HttpStatusCode)statusCode)
                : new HttpResponseMessage(HttpStatusCode.OK));
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, callCount); // initial + 2 retries
    }

    /// <summary>
    /// Verifies that transient network failures are retried for GET requests.
    /// </summary>
    [Fact]
    public async Task Get_HttpRequestException_IsRetried()
    {
        // Arrange: throw twice, succeed on third.
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            if (callCount < 3) throw new HttpRequestException("Network failure");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, callCount);
    }

    /// <summary>
    /// Verifies that timeout-style cancellations are retried for GET requests.
    /// </summary>
    [Fact]
    public async Task Get_TaskCanceledException_IsRetried()
    {
        // Arrange: simulate timeout-like cancellations twice, then succeed.
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            if (callCount < 3)
                throw new TaskCanceledException("Simulated timeout");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, callCount);
    }

    /// <summary>
    /// Verifies that the handler stops retrying after the configured maximum attempts.
    /// </summary>
    [Fact]
    public async Task Get_AlwaysFails_ThrowsAfterMaxRetries()
    {
        // Arrange: always throw.
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            throw new HttpRequestException("Always fails");
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync("/ping"));

        // 1 initial + 3 retries = 4 total calls.
        Assert.Equal(4, callCount);
    }
}
