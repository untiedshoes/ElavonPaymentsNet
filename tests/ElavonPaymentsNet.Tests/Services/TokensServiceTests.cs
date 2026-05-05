using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonTokensService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class TokensServiceTests
{
    /// <summary>
    /// Verifies that token creation posts to /token using Basic auth.
    /// </summary>
    [Fact(DisplayName = "CreateToken UsesExpectedRouteAndAuth")]
    public async Task CreateToken_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateTokensService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"token\":\"tok_1\"}");
        });

        var response = await service.CreateTokenAsync(new CreateTokenRequest
        {
            Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
        });

        Assert.Equal("tok_1", response.Token);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/token", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that pay-with-token posts to /transactions and maps token into the payload.
    /// </summary>
    [Fact(DisplayName = "PayWithToken UsesExpectedRouteAndPayload")]
    public async Task PayWithToken_UsesExpectedRouteAndPayload()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = ServiceTestHelpers.CreateTokensService(async request =>
        {
            captured     = request;
            capturedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-token\",\"status\":\"Ok\"}");
        });

        var response = await service.PayWithTokenAsync(new PayWithTokenRequest
        {
            VendorTxCode = "TX-T",
            Amount       = 200,
            Currency     = "GBP",
            Token        = "tok_abc"
        });

        Assert.Equal("tx-token", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal("/transactions", captured!.RequestUri!.AbsolutePath);
        Assert.NotNull(capturedBody);
        Assert.Contains("\"token\":\"tok_abc\"", capturedBody);
        Assert.Contains("\"transactionType\":\"Payment\"", capturedBody);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    [Fact(DisplayName = "CreateToken NullRequest ThrowsArgumentNullException")]
    public async Task CreateToken_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateTokensService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateTokenAsync(null!));
    }

    [Fact(DisplayName = "PayWithToken NullRequest ThrowsArgumentNullException")]
    public async Task PayWithToken_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateTokensService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.PayWithTokenAsync(null!));
    }
}
