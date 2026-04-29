using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Tests.Http.Fakes;
using System.Net;

namespace ElavonPaymentsNet.Tests.Http;

/// <summary>
/// Unit tests for <see cref="ElavonResilienceHandler"/>.
///
/// These tests verify non-retry behavior and constructor validation.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ElavonResilienceHandlerTests
{
    /// <summary>
    /// Verifies that an already-cancelled caller token fails immediately without retries.
    /// </summary>
    [Fact]
    public async Task Get_UserCancelledToken_IsNotRetried()
    {
        // Arrange: if the caller has already cancelled, request should fail immediately
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetAsync("/ping", cts.Token));

        Assert.Equal(0, callCount);
    }

    /// <summary>
    /// Verifies that client error responses are returned without retry attempts.
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(422)]
    public async Task Get_4xxResponse_IsNotRetried(int statusCode)
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)statusCode));
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

    /// <summary>
    /// Verifies that POST responses with server errors are never retried.
    /// </summary>
    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public async Task Post_5xxResponse_IsNeverRetried(int statusCode)
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)statusCode));
        });

        var handler = new ElavonResilienceHandler(maxRetryAttempts: 3) { InnerHandler = inner };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        // Act
        var response = await client.PostAsync("/transactions", new StringContent("{}"));

        // Assert — exactly one attempt, never retried
        Assert.Equal((HttpStatusCode)statusCode, response.StatusCode);
        Assert.Equal(1, callCount);
    }

    /// <summary>
    /// Verifies that POST network exceptions are propagated without retries.
    /// </summary>
    [Fact]
    public async Task Post_HttpRequestException_IsNeverRetried()
    {
        // Arrange
        var callCount = 0;
        var inner = new FakeHttpMessageHandler(_ =>
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
    // Constructor validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies constructor guards reject invalid retry attempt counts.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Constructor_InvalidRetryCount_Throws(int maxRetryAttempts)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ElavonResilienceHandler(maxRetryAttempts));
    }
}
