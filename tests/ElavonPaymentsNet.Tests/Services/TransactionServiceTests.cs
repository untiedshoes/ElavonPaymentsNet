using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonTransactionService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class TransactionServiceTests
{
    /// <summary>
    /// Verifies that transaction creation posts to /transactions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "CreateTransaction UsesExpectedRouteAndAuth")]
    public async Task CreateTransaction_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateTransactionService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-1\",\"status\":\"Ok\"}");
        });

        var response = await service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode    = "ORDER-1",
            Amount          = 100,
            Currency        = "GBP",
            PaymentMethod   = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }
        });

        Assert.Equal("tx-1", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
        Assert.Equal(ServiceTestHelpers.BasicParam(), captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that a bank-decline payload maps to NotAuthed semantics.
    /// </summary>
    [Fact(DisplayName = "CreateTransaction MapsBankDeclineResponse")]
    public async Task CreateTransaction_MapsBankDeclineResponse()
    {
        var service = ServiceTestHelpers.CreateTransactionService(async _ =>
            ServiceTestHelpers.Json(HttpStatusCode.OK,
            """
            {
              "transactionId":"tx-decline",
              "status":"NotAuthed",
              "statusCode":2000,
              "statusDetail":"The Authorisation was Declined by the bank.",
              "additionalDeclineDetail":{
                "additionalDeclineCode":"03",
                "additionalDeclineCodeDescription":"DECLINED",
                "additionalDeclineCodeCategory":"03"
              }
            }
            """));

        var response = await service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode    = "ORDER-DECLINE-1",
            Amount          = 100,
            Currency        = "GBP",
            PaymentMethod   = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929602110085639", ExpiryDate = "1229", SecurityCode = "123" }
            }
        });

        Assert.Equal("tx-decline", response.TransactionId);
        Assert.Equal("NotAuthed", response.Status);
        Assert.Equal(TransactionStatusKind.NotAuthed, response.StatusKind);
        Assert.Equal(2000, response.StatusCode);
        Assert.Equal("The Authorisation was Declined by the bank.", response.StatusDetail);
        Assert.NotNull(response.AdditionalDeclineDetail);
        Assert.Equal("03", response.AdditionalDeclineDetail!.AdditionalDeclineCode);
    }

    /// <summary>
    /// Verifies that SDK routes preserve the /api/v1 base path when BaseAddress has no trailing slash.
    /// </summary>
    [Fact(DisplayName = "CreateTransaction PreservesApiBasePath")]
    public async Task CreateTransaction_PreservesApiBasePath()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateTransactionService(
            async request =>
            {
                captured = request;
                return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-base\",\"status\":\"Ok\"}");
            },
            baseAddress: "https://example.com/api/v1");

        _ = await service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode    = "ORDER-BASE",
            Amount          = 100,
            Currency        = "GBP",
            PaymentMethod   = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }
        });

        Assert.NotNull(captured);
        Assert.Equal("/api/v1/transactions", captured!.RequestUri!.AbsolutePath);
    }

    /// <summary>
    /// Verifies that transaction retrieval sends GET /transactions/{id} using Basic auth.
    /// </summary>
    [Fact(DisplayName = "RetrieveTransaction UsesExpectedRouteAndVerb")]
    public async Task RetrieveTransaction_UsesExpectedRouteAndVerb()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateTransactionService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-2\",\"status\":\"Ok\"}");
        });

        var response = await service.RetrieveTransactionAsync("tx-2");

        Assert.Equal("tx-2", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/transactions/tx-2", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that null requests are rejected for transaction creation.
    /// </summary>
    [Fact(DisplayName = "CreateTransaction NullRequest ThrowsArgumentNullException")]
    public async Task CreateTransaction_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateTransactionService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-1\",\"status\":\"Ok\"}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateTransactionAsync(null!));
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected for transaction retrieval.
    /// </summary>
    [Fact(DisplayName = "RetrieveTransaction NullTransactionId ThrowsArgumentException")]
    public async Task RetrieveTransaction_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateTransactionService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-2\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.RetrieveTransactionAsync(null!));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "CreateTransaction BlankVendorTxCode ThrowsArgumentException")]
    public async Task CreateTransaction_BlankVendorTxCode_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateTransactionService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-1\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode    = " ",
            Amount          = 100,
            Currency        = "GBP",
            PaymentMethod   = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }
        }));

        Assert.Equal("VendorTxCode", ex.ParamName);
    }

    [Fact(DisplayName = "ReconcileUnknownCreateOutcome FoundMapping ReturnsTransaction")]
    public async Task ReconcileUnknownCreateOutcome_FoundMapping_ReturnsTransaction()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateTransactionService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-recon\",\"status\":\"Ok\"}");
        });

        var response = await service.ResolveUnknownTransactionAsync(
            "ORDER-RECON-1",
            (vendorTxCode, _) => Task.FromResult<string?>(vendorTxCode == "ORDER-RECON-1" ? "tx-recon" : null));

        Assert.NotNull(response);
        Assert.Equal("tx-recon", response!.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/transactions/tx-recon", captured.RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "ReconcileUnknownCreateOutcome MissingMapping ReturnsNull")]
    public async Task ReconcileUnknownCreateOutcome_MissingMapping_ReturnsNull()
    {
        var callCount = 0;
        var service = ServiceTestHelpers.CreateTransactionService(_ =>
        {
            callCount++;
            return Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx\",\"status\":\"Ok\"}"));
        });

        var response = await service.ResolveUnknownTransactionAsync(
            "ORDER-RECON-2",
            (_, _) => Task.FromResult<string?>(null));

        Assert.Null(response);
        Assert.Equal(0, callCount);
    }

    [Fact(DisplayName = "ReconcileUnknownCreateOutcome NullResolver ThrowsArgumentNullException")]
    public async Task ReconcileUnknownCreateOutcome_NullResolver_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateTransactionService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"transactionId\":\"tx\",\"status\":\"Ok\"}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ResolveUnknownTransactionAsync("ORDER-RECON-3", null!));
    }
}
