using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using ElavonPaymentsNet.Services;
using ElavonPaymentsNet.Tests.Http.Fakes;
using System.Net;
using System.Text;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for service-layer HTTP orchestration (method, route, auth, and response mapping).
/// </summary>
[Trait("Category", "Unit")]
public sealed class ElavonServicesTests
{
    /// <summary>
    /// Verifies that transaction creation posts to /transactions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Transactions CreateTransaction UsesExpectedRouteAndAuth")]
    public async Task Transactions_CreateTransaction_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateTransactionService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-1\",\"status\":\"Ok\"}");
        });

        var response = await service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "ORDER-1",
            Amount = 100,
            Currency = "GBP",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }
        });

        Assert.Equal("tx-1", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
        Assert.Equal(BasicParam(), captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that SDK routes preserve the /api/v1 base path when BaseAddress has no trailing slash.
    /// </summary>
    [Fact(DisplayName = "Transactions CreateTransaction PreservesApiBasePath")]
    public async Task Transactions_CreateTransaction_PreservesApiBasePath()
    {
        HttpRequestMessage? captured = null;
        var service = CreateTransactionService(
            async request =>
            {
                captured = request;
                return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-base\",\"status\":\"Ok\"}");
            },
            baseAddress: "https://example.com/api/v1");

        _ = await service.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "ORDER-BASE",
            Amount = 100,
            Currency = "GBP",
            PaymentMethod = new PaymentMethod
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
    [Fact(DisplayName = "Transactions RetrieveTransaction UsesExpectedRouteAndVerb")]
    public async Task Transactions_RetrieveTransaction_UsesExpectedRouteAndVerb()
    {
        HttpRequestMessage? captured = null;
        var service = CreateTransactionService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-2\",\"status\":\"Ok\"}");
        });

        var response = await service.RetrieveTransactionAsync("tx-2");

        Assert.Equal("tx-2", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/transactions/tx-2", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that capture posts to /transactions/{id}/capture using Basic auth.
    /// </summary>
    [Fact(DisplayName = "PostPayments Capture UsesExpectedRouteAndAuth")]
    public async Task PostPayments_Capture_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = CreatePostPaymentService(async request =>
        {
            captured = request;
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            return Json(HttpStatusCode.OK, "{\"instructionType\":\"Release\",\"date\":\"2026-01-01T00:00:00Z\"}");
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
    /// Verifies that refund posts to /transactions/{id}/refund using Basic auth.
    /// </summary>
    [Fact(DisplayName = "PostPayments Refund UsesExpectedRouteAndAuth")]
    public async Task PostPayments_Refund_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = CreatePostPaymentService(async request =>
        {
            captured = request;
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-r\",\"status\":\"Ok\"}");
        });

        var response = await service.RefundTransactionAsync("tx-123", new RefundPaymentRequest
        {
            Amount = 25,
            VendorTxCode = "R-1",
            Description = "Refund service test"
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
    /// Verifies that void posts to /transactions/{id}/void using Basic auth.
    /// </summary>
    [Fact(DisplayName = "PostPayments Void UsesExpectedRouteAndAuth")]
    public async Task PostPayments_Void_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = CreatePostPaymentService(async request =>
        {
            captured = request;
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            return Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
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

    /// <summary>
    /// Verifies that 3DS challenge completion posts to /transactions/{id}/3d-secure-challenge using Basic auth.
    /// </summary>
    [Fact(DisplayName = "ThreeDs Complete UsesExpectedRouteAndAuth")]
    public async Task ThreeDs_Complete_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateThreeDsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-3ds\",\"status\":\"Ok\"}");
        });

        var response = await service.Complete3DsAsync("tx-3ds", new Complete3DsRequest { CRes = "cres-value" });

        Assert.Equal("tx-3ds", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/transactions/tx-3ds/3d-secure-challenge", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that token creation posts to /token using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Tokens CreateToken UsesExpectedRouteAndAuth")]
    public async Task Tokens_CreateToken_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateTokensService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"token\":\"tok_1\"}");
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
    [Fact(DisplayName = "Tokens PayWithToken UsesExpectedRouteAndPayload")]
    public async Task Tokens_PayWithToken_UsesExpectedRouteAndPayload()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var service = CreateTokensService(async request =>
        {
            captured = request;
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            return Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-token\",\"status\":\"Ok\"}");
        });

        var response = await service.PayWithTokenAsync(new PayWithTokenRequest
        {
            VendorTxCode = "TX-T",
            Amount = 200,
            Currency = "GBP",
            Token = "tok_abc"
        });

        Assert.Equal("tx-token", response.TransactionId);
        Assert.NotNull(captured);
        Assert.Equal("/transactions", captured!.RequestUri!.AbsolutePath);
        Assert.NotNull(capturedBody);
        Assert.Contains("\"token\":\"tok_abc\"", capturedBody);
        Assert.Contains("\"transactionType\":\"Payment\"", capturedBody);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that merchant session key creation posts to /merchant-session-keys using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Wallets CreateMerchantSessionKey UsesExpectedRouteAndAuth")]
    public async Task Wallets_CreateMerchantSessionKey_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateWalletsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"merchantSessionKey\":\"msk_1\"}");
        });

        var response = await service.CreateMerchantSessionKeyAsync(new MerchantSessionRequest());

        Assert.Equal("msk_1", response.MerchantSessionKey);
        Assert.NotNull(captured);
        Assert.Equal("/merchant-session-keys", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that merchant session validation gets /merchant-session-keys/{key} using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Wallets ValidateMerchantSessionKey UsesExpectedRouteAndAuth")]
    public async Task Wallets_ValidateMerchantSessionKey_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateWalletsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"merchantSessionKey\":\"msk_1\",\"expiry\":\"2025-01-01T00:00:00Z\"}");
        });

        var response = await service.ValidateMerchantSessionKeyAsync(new MerchantSessionValidationRequest { MerchantSessionKey = "msk_1" });

        Assert.True(response.Valid);
        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/merchant-session-keys/msk_1", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that Apple Pay session creation posts to /applepay/sessions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Wallets CreateApplePaySession UsesExpectedRouteAndAuth")]
    public async Task Wallets_CreateApplePaySession_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateWalletsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"session\":{\"merchantSessionIdentifier\":\"ms\"}}");
        });

        var response = await service.CreateApplePaySessionAsync(new ApplePaySessionRequest { ValidationUrl = "https://apple/validate", DomainName = "merchant.test" });

        Assert.True(response.Session.HasValue);
        Assert.Equal("ms", response.Session.Value.GetProperty("merchantSessionIdentifier").GetString());
        Assert.NotNull(captured);
        Assert.Equal("/applepay/sessions", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that card identifier creation uses Bearer auth and posts to /card-identifiers.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers Create UsesBearerAuth")]
    public async Task CardIdentifiers_Create_UsesBearerAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"cardIdentifier\":\"cid_1\"}");
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
    /// Verifies that linking security code uses Basic auth and the expected card-identifier route.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers LinkSecurityCode UsesBasicAuth")]
    public async Task CardIdentifiers_LinkSecurityCode_UsesBasicAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{}");
        });

        await service.LinkCardIdentifierAsync("cid_1", new LinkCardIdentifierRequest { SecurityCode = "123" });

        Assert.NotNull(captured);
        Assert.Equal("/card-identifiers/cid_1/security-code", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
        Assert.Equal(BasicParam(), captured.Headers.Authorization!.Parameter);
    }

    /// <summary>
    /// Verifies that blank transaction identifiers are rejected before sending a capture request.
    /// </summary>
    [Fact(DisplayName = "PostPayments Capture BlankTransactionId ThrowsArgumentException")]
    public async Task PostPayments_Capture_BlankTransactionId_ThrowsArgumentException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-c\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CaptureTransactionAsync(" ", new CapturePaymentRequest { Amount = 50 }));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected before sending a capture request.
    /// </summary>
    [Fact(DisplayName = "PostPayments Capture NullTransactionId ThrowsArgumentException")]
    public async Task PostPayments_Capture_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-c\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CaptureTransactionAsync(null!, new CapturePaymentRequest { Amount = 50 }));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected before sending a refund request.
    /// </summary>
    [Fact(DisplayName = "PostPayments Refund NullTransactionId ThrowsArgumentException")]
    public async Task PostPayments_Refund_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-r\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RefundTransactionAsync(null!, new RefundPaymentRequest { Amount = 25, VendorTxCode = "R-1", Description = "Refund null transactionId test" }));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected before sending a void request.
    /// </summary>
    [Fact(DisplayName = "PostPayments Void NullTransactionId ThrowsArgumentException")]
    public async Task PostPayments_Void_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.VoidTransactionAsync(null!));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected for transaction retrieval.
    /// </summary>
    [Fact(DisplayName = "Transactions RetrieveTransaction NullTransactionId ThrowsArgumentException")]
    public async Task Transactions_RetrieveTransaction_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreateTransactionService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-2\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RetrieveTransactionAsync(null!));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected for 3DS completion.
    /// </summary>
    [Fact(DisplayName = "ThreeDs Complete NullTransactionId ThrowsArgumentException")]
    public async Task ThreeDs_Complete_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreateThreeDsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-3ds\",\"status\":\"Ok\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.Complete3DsAsync(null!, new Complete3DsRequest { CRes = "cres-value" }));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null transaction identifiers are rejected for instruction creation.
    /// </summary>
    [Fact(DisplayName = "Instructions Create NullTransactionId ThrowsArgumentException")]
    public async Task Instructions_Create_NullTransactionId_ThrowsArgumentException()
    {
        var service = CreateInstructionsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInstructionAsync(null!, new InstructionRequest { InstructionType = InstructionType.Void }));

        Assert.Equal("transactionId", ex.ParamName);
    }

    /// <summary>
    /// Verifies that null requests are rejected for transaction creation.
    /// </summary>
    [Fact(DisplayName = "Transactions CreateTransaction NullRequest ThrowsArgumentNullException")]
    public async Task Transactions_CreateTransaction_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateTransactionService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"transactionId\":\"tx-1\",\"status\":\"Ok\"}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTransactionAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for capture operations.
    /// </summary>
    [Fact(DisplayName = "PostPayments Capture NullRequest ThrowsArgumentNullException")]
    public async Task PostPayments_Capture_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CaptureTransactionAsync("tx-123", null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for refund operations.
    /// </summary>
    [Fact(DisplayName = "PostPayments Refund NullRequest ThrowsArgumentNullException")]
    public async Task PostPayments_Refund_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreatePostPaymentService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RefundTransactionAsync("tx-123", null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for 3DS completion.
    /// </summary>
    [Fact(DisplayName = "ThreeDs Complete NullRequest ThrowsArgumentNullException")]
    public async Task ThreeDs_Complete_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateThreeDsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.Complete3DsAsync("tx-123", null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for token creation.
    /// </summary>
    [Fact(DisplayName = "Tokens CreateToken NullRequest ThrowsArgumentNullException")]
    public async Task Tokens_CreateToken_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateTokensService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTokenAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for pay-with-token operations.
    /// </summary>
    [Fact(DisplayName = "Tokens PayWithToken NullRequest ThrowsArgumentNullException")]
    public async Task Tokens_PayWithToken_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateTokensService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.PayWithTokenAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for merchant-session key creation.
    /// </summary>
    [Fact(DisplayName = "Wallets CreateMerchantSessionKey NullRequest ThrowsArgumentNullException")]
    public async Task Wallets_CreateMerchantSessionKey_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateWalletsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateMerchantSessionKeyAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for merchant-session key validation.
    /// </summary>
    [Fact(DisplayName = "Wallets ValidateMerchantSessionKey NullRequest ThrowsArgumentNullException")]
    public async Task Wallets_ValidateMerchantSessionKey_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateWalletsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ValidateMerchantSessionKeyAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for Apple Pay session creation.
    /// </summary>
    [Fact(DisplayName = "Wallets CreateApplePaySession NullRequest ThrowsArgumentNullException")]
    public async Task Wallets_CreateApplePaySession_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateWalletsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateApplePaySessionAsync(null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for instruction creation.
    /// </summary>
    [Fact(DisplayName = "Instructions Create NullRequest ThrowsArgumentNullException")]
    public async Task Instructions_Create_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateInstructionsService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateInstructionAsync("tx-123", null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for card identifier creation.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers Create NullRequest ThrowsArgumentNullException")]
    public async Task CardIdentifiers_Create_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateCardIdentifiersService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateCardIdentifierAsync("msk_123", null!));
    }

    /// <summary>
    /// Verifies that null requests are rejected for security-code linking.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers LinkSecurityCode NullRequest ThrowsArgumentNullException")]
    public async Task CardIdentifiers_LinkSecurityCode_NullRequest_ThrowsArgumentNullException()
    {
        var service = CreateCardIdentifiersService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.LinkCardIdentifierAsync("cid_123", null!));
    }

    /// <summary>
    /// Verifies that transaction identifiers are URI-escaped before building instruction routes.
    /// </summary>
    [Fact(DisplayName = "Instructions Create EscapesTransactionIdInRoute")]
    public async Task Instructions_Create_EscapesTransactionIdInRoute()
    {
        HttpRequestMessage? captured = null;
        var service = CreateInstructionsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        await service.CreateInstructionAsync("tx/with space", new InstructionRequest { InstructionType = InstructionType.Void });

        Assert.NotNull(captured);
        Assert.Equal("/transactions/tx%2Fwith%20space/instructions", captured!.RequestUri!.AbsolutePath);
    }

    /// <summary>
    /// Verifies that blank merchant session keys are rejected before creating card identifiers.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers Create BlankMerchantSessionKey ThrowsArgumentException")]
    public async Task CardIdentifiers_Create_BlankMerchantSessionKey_ThrowsArgumentException()
    {
        var service = CreateCardIdentifiersService(_ => Task.FromResult(Json(HttpStatusCode.OK, "{\"cardIdentifier\":\"cid_1\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCardIdentifierAsync(" ", new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1229" }
            }));

        Assert.Equal("merchantSessionKey", ex.ParamName);
    }

    /// <summary>
    /// Verifies that card identifiers are URI-escaped before building the security-code route.
    /// </summary>
    [Fact(DisplayName = "CardIdentifiers LinkSecurityCode EscapesCardIdentifierInRoute")]
    public async Task CardIdentifiers_LinkSecurityCode_EscapesCardIdentifierInRoute()
    {
        HttpRequestMessage? captured = null;
        var service = CreateCardIdentifiersService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{}");
        });

        await service.LinkCardIdentifierAsync("cid/with space", new LinkCardIdentifierRequest { SecurityCode = "123" });

        Assert.NotNull(captured);
        Assert.Equal("/card-identifiers/cid%2Fwith%20space/security-code", captured!.RequestUri!.AbsolutePath);
    }

    /// <summary>
    /// Verifies that instruction creation posts to /transactions/{id}/instructions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Instructions Create UsesExpectedRouteAndAuth")]
    public async Task Instructions_Create_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = CreateInstructionsService(async request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        var response = await service.CreateInstructionAsync("tx-1", new InstructionRequest { InstructionType = InstructionType.Void });

        Assert.Equal(InstructionType.Void, response.InstructionType);
        Assert.NotNull(captured);
        Assert.Equal("/transactions/tx-1/instructions", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    private static ElavonTransactionService CreateTransactionService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        string baseAddress = "https://example.com")
    {
        var api = CreateApi(responder, baseAddress);
        return new ElavonTransactionService(api);
    }

    private static ElavonPostPaymentService CreatePostPaymentService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonPostPaymentService(api);
    }

    private static ElavonThreeDsService CreateThreeDsService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonThreeDsService(api);
    }

    private static ElavonTokensService CreateTokensService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonTokensService(api);
    }

    private static ElavonWalletsService CreateWalletsService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonWalletsService(api);
    }

    private static ElavonCardIdentifiersService CreateCardIdentifiersService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonCardIdentifiersService(api);
    }

    private static ElavonInstructionsService CreateInstructionsService(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
    {
        var api = CreateApi(responder);
        return new ElavonInstructionsService(api);
    }

    private static ElavonApiClient CreateApi(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        string baseAddress = "https://example.com")
    {
        // Wire the auth handler in front of the fake so auth assertions work.
        var authHandler = new ElavonAuthenticationHandler("ik", "ip")
        {
            InnerHandler = new FakeHttpMessageHandler(responder)
        };
        var httpClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri(baseAddress)
        };
        return new ElavonApiClient(httpClient);
    }

    private static string BasicParam() => Convert.ToBase64String(Encoding.ASCII.GetBytes("ik:ip"));

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
        => new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
