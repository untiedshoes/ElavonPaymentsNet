using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonCardIdentifiersService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class CardIdentifiersServiceTests
{
    /// <summary>
    /// Verifies that card identifier creation uses Bearer auth and posts to /card-identifiers.
    /// </summary>
    [Fact(DisplayName = "Create UsesBearerAuth")]
    public async Task Create_UsesBearerAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"cardIdentifier\":\"cid_1\"}");
        });

        var response = await service.CreateCardIdentifierAsync("msk_123", new CreateCardIdentifierRequest
        {
            CardDetails = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
        });

        Assert.Equal("cid_1", response.CardIdentifier);
        Assert.NotNull(captured);
        Assert.Equal("/card-identifiers", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("msk_123", captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that linking a security code uses Bearer auth and the expected route.
    /// </summary>
    [Fact(DisplayName = "LinkSecurityCode UsesBearerAuth")]
    public async Task LinkSecurityCode_UsesBearerAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{}");
        });

        await service.LinkCardIdentifierAsync("msk_123", "cid_1", new LinkCardIdentifierRequest { SecurityCode = "123" });

        Assert.NotNull(captured);
        Assert.Equal("/card-identifiers/cid_1/security-code", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("msk_123", captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that removing a card identifier uses DELETE with Basic auth.
    /// </summary>
    [Fact(DisplayName = "Remove UsesBasicAuth")]
    public async Task Remove_UsesBasicAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.NoContent, string.Empty);
        });

        await service.RemoveCardIdentifierAsync("cid_1");

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/card-identifiers/cid_1", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
        Assert.Equal(ServiceTestHelpers.BasicParam(), captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that card identifiers are URI-escaped in the security-code route.
    /// </summary>
    [Fact(DisplayName = "LinkSecurityCode EscapesCardIdentifierInRoute")]
    public async Task LinkSecurityCode_EscapesCardIdentifierInRoute()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{}");
        });

        await service.LinkCardIdentifierAsync("msk_123", "cid/with space", new LinkCardIdentifierRequest { SecurityCode = "123" });

        Assert.NotNull(captured);
        Assert.Equal("/card-identifiers/cid%2Fwith%20space/security-code", captured!.RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "Remove NullCardIdentifier ThrowsArgumentException")]
    public async Task Remove_NullCardIdentifier_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateCardIdentifiersService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.NoContent, string.Empty)));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.RemoveCardIdentifierAsync(null!));
        Assert.Equal("cardIdentifier", ex.ParamName);
    }

    [Fact(DisplayName = "Create BlankMerchantSessionKey ThrowsArgumentException")]
    public async Task Create_BlankMerchantSessionKey_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateCardIdentifiersService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"cardIdentifier\":\"cid_1\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCardIdentifierAsync(" ", new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }));
        Assert.Equal("merchantSessionKey", ex.ParamName);
    }

    [Fact(DisplayName = "Create NullRequest ThrowsArgumentNullException")]
    public async Task Create_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateCardIdentifiersService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateCardIdentifierAsync("msk_123", null!));
    }

    [Fact(DisplayName = "LinkSecurityCode NullRequest ThrowsArgumentNullException")]
    public async Task LinkSecurityCode_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateCardIdentifiersService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.LinkCardIdentifierAsync("msk_123", "cid_123", null!));
    }
}
