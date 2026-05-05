using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonThreeDsService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ThreeDsServiceTests
{
    /// <summary>
    /// Verifies that 3DS challenge completion posts to /transactions/{id}/3d-secure-challenge using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Complete UsesExpectedRouteAndAuth")]
    public async Task Complete_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateThreeDsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-3ds\",\"status\":\"Ok\"}");
        });

        var response = await service.Complete3DsAsync("tx-3ds", new Complete3DsRequest { CRes = "cres-value" });

        Assert.Equal("tx-3ds", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions/tx-3ds/3d-secure-challenge", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    [Fact(DisplayName = "Complete NullTransactionId ThrowsArgumentException")]
    public async Task Complete_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateThreeDsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-3ds\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.Complete3DsAsync(null!, new Complete3DsRequest { CRes = "cres-value" }));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Complete NullRequest ThrowsArgumentNullException")]
    public async Task Complete_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateThreeDsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Complete3DsAsync("tx-123", null!));
    }
}
