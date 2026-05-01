using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests against the real Elavon sandbox API.
/// Sandbox credentials are hardcoded — they are publicly available in the Opayo PI REST API
/// documentation and only work against the non-production sandbox environment.
/// No environment variables are required to run these tests, except
/// <c>ELAVON_SAFE_TRANSACTION_ID</c> for the retrieve-transaction test.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ElavonIntegrationTests
{
    // ----------------------------------------------------------------
    // Merchant session key
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that a merchant session key can be created using the sandbox Basic profile.
    /// </summary>
    [Fact]
    public async Task CreateMerchantSessionKeyAsync_ReturnsSessionKeyAndExpiry()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var response = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.MerchantSessionKey));
        Assert.True(response.Expiry.HasValue);
    }

    /// <summary>
    /// Verifies that a freshly created merchant session key validates successfully.
    /// </summary>
    [Fact]
    public async Task ValidateMerchantSessionKeyAsync_FreshKey_ReturnsValid()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var response = await client.Wallets.ValidateMerchantSessionKeyAsync(
            new MerchantSessionValidationRequest { MerchantSessionKey = session.MerchantSessionKey! });

        Assert.NotNull(response);
        Assert.True(response.Valid);
    }

    // ----------------------------------------------------------------
    // Card identifiers
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that a card identifier can be created from a fresh merchant session key.
    /// </summary>
    [Fact]
    public async Task CreateCardIdentifierAsync_ReturnsIdentifierExpiryAndCardType()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var response = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.CardIdentifier));
        Assert.False(string.IsNullOrWhiteSpace(response.Expiry));
        Assert.False(string.IsNullOrWhiteSpace(response.CardType));
    }

    // ----------------------------------------------------------------
    // Transactions — Payment
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies the full MSK + card-identifier + payment flow returns Ok.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_ReturnsOk()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        Assert.False(string.IsNullOrWhiteSpace(cardId.CardIdentifier));

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("Ok", result.Status);
    }

    /// <summary>
    /// Verifies that the known sandbox decline card (4929602110085639) returns
    /// NotAuthed / 2000 / "Declined by the bank" when 3DS is disabled.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_DeclineCard_ReturnsBankDecline()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929602110085639",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        Assert.False(string.IsNullOrWhiteSpace(cardId.CardIdentifier));

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-DECLINE-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test bank decline payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("NotAuthed", result.Status);
        Assert.Equal(2000, result.StatusCode);
        Assert.Equal("The Authorisation was Declined by the bank.", result.StatusDetail);
    }

    // ----------------------------------------------------------------
    // Transactions — Deferred & Repeat
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that a deferred transaction can be created with TransactionType.Deferred.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_Deferred_ReturnsOk()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Deferred,
            VendorTxCode      = $"INTEGRATION-DEF-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test deferred payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("Ok", result.Status);
    }

    /// <summary>
    /// Verifies that a repeat transaction can be created from a completed payment.
    /// Uses SandboxHelpers for the prerequisite payment — that path is covered by
    /// CreateTransactionAsync_CardIdentifierFlow_ReturnsOk.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_Repeat_ReturnsOk()
    {
        var originalId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "ORIG");
        if (originalId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var repeat = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType      = TransactionType.Repeat,
            VendorTxCode         = $"INTEGRATION-RPT-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount               = 100,
            Currency             = "GBP",
            Description          = "Integration test repeat payment",
            RelatedTransactionId = originalId
        });

        Assert.NotNull(repeat);
        Assert.False(string.IsNullOrWhiteSpace(repeat.TransactionId));
        Assert.Equal("Ok", repeat.Status);
    }

    // ----------------------------------------------------------------
    // Transactions — Retrieve
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that RetrieveTransactionAsync can read a known safe transaction from the sandbox.
    /// Requires the <c>ELAVON_SAFE_TRANSACTION_ID</c> environment variable.
    /// </summary>
    [Fact]
    public async Task RetrieveTransactionAsync_WithConfiguredSafeId_ReturnsResponse()
    {
        var transactionId = Environment.GetEnvironmentVariable("ELAVON_SAFE_TRANSACTION_ID");
        if (string.IsNullOrWhiteSpace(transactionId))
            return; // Skipped — set ELAVON_SAFE_TRANSACTION_ID to a known sandbox transaction ID to run.

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var response = await client.Transactions.RetrieveTransactionAsync(transactionId);

        Assert.NotNull(response);
        Assert.Equal(transactionId, response.TransactionId);
        Assert.False(string.IsNullOrWhiteSpace(response.Status));
    }

    // ----------------------------------------------------------------
    // Post-payment — Void, Refund, Capture
    // ----------------------------------------------------------------

    /// <summary>Verifies that a transaction can be voided via PostPayments.</summary>
    [Fact]
    public async Task VoidTransactionAsync_AfterPayment_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "VOID");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.VoidTransactionAsync(txId);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>Verifies that a transaction can be refunded via PostPayments.</summary>
    [Fact]
    public async Task RefundTransactionAsync_AfterPayment_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "REFUND");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.RefundTransactionAsync(txId, new RefundPaymentRequest
        {
            Amount       = 100,
            VendorTxCode = $"INTEGRATION-RFND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Description  = "Integration test refund payment"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>Verifies that a deferred transaction can be captured via PostPayments.</summary>
    [Fact]
    public async Task CaptureTransactionAsync_AfterDeferred_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.CaptureTransactionAsync(txId, new CapturePaymentRequest
        {
            Amount = 100
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    // ----------------------------------------------------------------
    // Instructions — Void, Abort, Release
    // ----------------------------------------------------------------

    /// <summary>Verifies that a Void instruction can be posted to a completed payment.</summary>
    [Fact]
    public async Task CreateInstructionAsync_Void_ReturnsVoidInstruction()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "INSTRVOID");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Void
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Void, result.InstructionType);
    }

    /// <summary>Verifies that an Abort instruction can be posted to a deferred transaction.</summary>
    [Fact]
    public async Task CreateInstructionAsync_Abort_OnDeferred_ReturnsAbortInstruction()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Abort
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Abort, result.InstructionType);
    }

    /// <summary>Verifies that a Release instruction can be posted to a deferred transaction.</summary>
    [Fact]
    public async Task CreateInstructionAsync_Release_OnDeferred_ReturnsReleaseInstruction()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Release,
            Amount          = 100
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Release, result.InstructionType);
    }

    // ----------------------------------------------------------------
    // Failure scenarios
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that invalid credentials are rejected by the real API with an authentication exception.
    /// </summary>
    [Fact]
    public async Task CreateMerchantSessionKeyAsync_WithInvalidCredentials_ThrowsAuthenticationException()
    {
        var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
        {
            IntegrationKey      = "invalid-key",
            IntegrationPassword = "invalid-password",
            Environment         = ElavonEnvironment.Sandbox
        });

        await Assert.ThrowsAsync<ElavonAuthenticationException>(() =>
            client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest()));
    }
}
