using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonWalletsService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class WalletsServiceTests
{
    /// <summary>
    /// Verifies that merchant session key creation posts to /merchant-session-keys using Basic auth.
    /// </summary>
    [Fact(DisplayName = "CreateMerchantSessionKey UsesExpectedRouteAndAuth")]
    public async Task CreateMerchantSessionKey_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateWalletsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"merchantSessionKey\":\"msk_1\"}");
        });

        var response = await service.CreateMerchantSessionKeyAsync(new MerchantSessionRequest());

        Assert.Equal("msk_1", response.MerchantSessionKey);
        Assert.NotNull(captured);
        Assert.Equal("/merchant-session-keys", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that merchant session validation GETs /merchant-session-keys/{key} using Basic auth.
    /// </summary>
    [Fact(DisplayName = "ValidateMerchantSessionKey UsesExpectedRouteAndAuth")]
    public async Task ValidateMerchantSessionKey_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateWalletsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"merchantSessionKey\":\"msk_1\",\"expiry\":\"2025-01-01T00:00:00Z\"}");
        });

        var response = await service.ValidateMerchantSessionKeyAsync(
            new MerchantSessionValidationRequest { MerchantSessionKey = "msk_1" });

        Assert.True(response.Valid);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/merchant-session-keys/msk_1", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that Apple Pay session creation posts to /applepay/sessions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "CreateApplePaySession UsesExpectedRouteAndAuth")]
    public async Task CreateApplePaySession_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateWalletsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK,
                "{\"sessionValidationToken\":\"tok-abc\",\"merchantSessionIdentifier\":\"ms-123\",\"status\":\"Ok\"}");
        });

        var response = await service.CreateApplePaySessionAsync(
            new ApplePaySessionRequest { VendorName = "sandbox", Domain = "merchant.test" });

        Assert.Equal("tok-abc", response.SessionValidationToken);
        Assert.Equal("ms-123", response.MerchantSessionIdentifier);
        Assert.NotNull(captured);
        Assert.Equal("/applepay/sessions", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    [Fact(DisplayName = "CreateMerchantSessionKey NullRequest ThrowsArgumentNullException")]
    public async Task CreateMerchantSessionKey_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateWalletsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateMerchantSessionKeyAsync(null!));
    }

    [Fact(DisplayName = "ValidateMerchantSessionKey NullRequest ThrowsArgumentNullException")]
    public async Task ValidateMerchantSessionKey_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateWalletsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ValidateMerchantSessionKeyAsync(null!));
    }

    [Fact(DisplayName = "CreateApplePaySession NullRequest ThrowsArgumentNullException")]
    public async Task CreateApplePaySession_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateWalletsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateApplePaySessionAsync(null!));
    }
}
