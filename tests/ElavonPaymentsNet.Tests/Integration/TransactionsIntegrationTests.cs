namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Transactions service against the real Elavon sandbox API.
/// Covers Payment, Deferred, Repeat, and Retrieve operations, plus bank decline simulation.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TransactionsIntegrationTests
{
    // ----------------------------------------------------------------
    // Payment
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies the full MSK + card-identifier + payment flow returns Ok.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionAsync CardIdentifierFlow ReturnsOk")]
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
    [Fact(DisplayName = "CreateTransactionAsync DeclineCard ReturnsBankDecline")]
    public async Task CreateTransactionAsync_DeclineCard_ReturnsBankDecline()
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
    // Deferred
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that a deferred transaction can be created with TransactionType.Deferred.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionAsync Deferred ReturnsOk")]
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

    // ----------------------------------------------------------------
    // Repeat
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that a repeat transaction can be created from a completed payment.
    /// Uses SandboxHelpers for the prerequisite payment — that path is covered by
    /// CreateTransactionAsync_CardIdentifierFlow_ReturnsOk.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionAsync Repeat ReturnsOk")]
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
    // Retrieve
    // ----------------------------------------------------------------

    /// <summary>
    /// Verifies that RetrieveTransactionAsync can read a known safe transaction from the sandbox.
    /// Requires the <c>ELAVON_SAFE_TRANSACTION_ID</c> environment variable.
    /// </summary>
    [Fact(DisplayName = "RetrieveTransactionAsync WithConfiguredSafeId ReturnsResponse")]
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

    /// <summary>
    /// Verifies that a freshly created transaction can be retrieved by its transaction ID.
    /// </summary>
    [Fact(DisplayName = "RetrieveTransactionAsync AfterPayment ReturnsMatchingTransaction")]
    public async Task RetrieveTransactionAsync_AfterPayment_ReturnsMatchingTransaction()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "RETRIEVE");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var response = await client.Transactions.RetrieveTransactionAsync(txId);

        Assert.NotNull(response);
        Assert.Equal(txId, response.TransactionId);
        Assert.Equal("Ok", response.Status);
    }
}
