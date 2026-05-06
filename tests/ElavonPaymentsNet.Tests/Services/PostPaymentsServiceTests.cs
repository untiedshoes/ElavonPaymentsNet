using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonPostPaymentService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class PostPaymentsServiceTests
{
    /// <summary>
    /// Verifies that capture posts to /transactions/{id}/instructions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Capture UsesExpectedRouteAndAuth")]
    public async Task Capture_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = ServiceTestHelpers.CreatePostPaymentService(async request =>
        {
            captured    = request;
            capturedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructionType\":\"Release\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        var response = await service.CaptureTransactionAsync("tx-123", new CapturePaymentRequest { Amount = 50 });

        Assert.Equal("tx-123", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions/tx-123/instructions", captured.RequestUri!.AbsolutePath);
        Assert.NotNull(capturedBody);
        Assert.Contains("\"instructionType\":\"release\"", capturedBody);
        Assert.Contains("\"amount\":50", capturedBody);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that refund posts to /transactions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Refund UsesExpectedRouteAndAuth")]
    public async Task Refund_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = ServiceTestHelpers.CreatePostPaymentService(async request =>
        {
            captured     = request;
            capturedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-r\",\"status\":\"Ok\"}");
        });

        var response = await service.RefundTransactionAsync("tx-123", new RefundPaymentRequest
        {
            Amount       = 25,
            VendorTxCode = "R-1",
            Description  = "Refund service test"
        });

        Assert.Equal("tx-r", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions", captured.RequestUri!.AbsolutePath);
        Assert.NotNull(capturedBody);
        Assert.Contains("\"transactionType\":\"Refund\"", capturedBody);
        Assert.Contains("\"description\":\"Refund service test\"", capturedBody);
        Assert.Contains("\"referenceTransactionId\":\"tx-123\"", capturedBody);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that void posts to /transactions/{id}/instructions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Void UsesExpectedRouteAndAuth")]
    public async Task Void_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = ServiceTestHelpers.CreatePostPaymentService(async request =>
        {
            captured     = request;
            capturedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        var response = await service.VoidTransactionAsync("tx-123");

        Assert.Equal("tx-123", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions/tx-123/instructions", captured.RequestUri!.AbsolutePath);
        Assert.NotNull(capturedBody);
        Assert.Contains("\"instructionType\":\"void\"", capturedBody);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    [Fact(DisplayName = "Capture BlankTransactionId ThrowsArgumentException")]
    public async Task Capture_BlankTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-c\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CaptureTransactionAsync(" ", new CapturePaymentRequest { Amount = 50 }));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Capture NullTransactionId ThrowsArgumentException")]
    public async Task Capture_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-c\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CaptureTransactionAsync(null!, new CapturePaymentRequest { Amount = 50 }));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Refund NullTransactionId ThrowsArgumentException")]
    public async Task Refund_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-r\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RefundTransactionAsync(null!, new RefundPaymentRequest { Amount = 25, VendorTxCode = "R-1", Description = "test" }));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Void NullTransactionId ThrowsArgumentException")]
    public async Task Void_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.VoidTransactionAsync(null!));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Capture NullRequest ThrowsArgumentNullException")]
    public async Task Capture_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CaptureTransactionAsync("tx-123", null!));
    }

    [Fact(DisplayName = "Refund NullRequest ThrowsArgumentNullException")]
    public async Task Refund_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RefundTransactionAsync("tx-123", null!));
    }

    [Fact(DisplayName = "Refund BlankVendorTxCode ThrowsArgumentException")]
    public async Task Refund_BlankVendorTxCode_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreatePostPaymentService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.RefundTransactionAsync("tx-123", new RefundPaymentRequest
        {
            Amount       = 25,
            VendorTxCode = " ",
            Description  = "refund"
        }));

        Assert.Equal("VendorTxCode", ex.ParamName);
    }
}
