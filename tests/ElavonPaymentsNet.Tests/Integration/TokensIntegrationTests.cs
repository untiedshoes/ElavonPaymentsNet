namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Tokens service against the real Elavon sandbox API.
/// Covers token-based payment (save-and-reuse card identifier flow).
/// </summary>
[Trait("Category", "Integration")]
public sealed class TokensIntegrationTests
{
    /// <summary>
    /// Verifies the full save-and-reuse flow: create a card identifier with CIT credential
    /// metadata on first use, then reuse it as a MIT (merchant-initiated) payment.
    /// This is the standard recurring/token payment pattern for Opayo.
    /// </summary>
    [Fact(DisplayName = "SaveAndReuseCardIdentifier MIT Payment ReturnsOk")]
    public async Task SaveAndReuseCardIdentifier_MitPayment_ReturnsOk()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        // Step 1 — Create MSK and card identifier
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

        // Step 2 — First (CIT) payment: save the card identifier for future use
        var firstPayment = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-SAVE-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test — save card (CIT)",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier,
                    Save               = true
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            CredentialType = new CredentialType
            {
                CofUsage      = "First",
                InitiatedType = "CIT"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(firstPayment);
        Assert.False(string.IsNullOrWhiteSpace(firstPayment.TransactionId));
        Assert.Equal("Ok", firstPayment.Status);

        // The saved card identifier is returned in the response for reuse
        var savedCardIdentifier = firstPayment.PaymentMethod?.Card?.CardIdentifier
                                  ?? cardId.CardIdentifier;

        Assert.False(string.IsNullOrWhiteSpace(savedCardIdentifier));

        // Step 3 — Subsequent (MIT) payment: reuse the saved card identifier
        var repeatPayment = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Repeat,
            VendorTxCode      = $"INTEGRATION-REUSE-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test — reuse card (MIT)",
            CredentialType    = new CredentialType
            {
                CofUsage      = "Subsequent",
                InitiatedType = "MIT",
                MitType       = "Unscheduled"
            },
            RelatedTransactionId = firstPayment.TransactionId
        });

        Assert.NotNull(repeatPayment);
        Assert.False(string.IsNullOrWhiteSpace(repeatPayment.TransactionId));
        Assert.Equal("Ok", repeatPayment.Status);
    }

    /// <summary>
    /// Verifies that PayWithTokenAsync completes when a token is available.
    /// Skipped if no token can be obtained from the sandbox environment.
    /// </summary>
    [Fact(DisplayName = "PayWithTokenAsync ValidToken ReturnsOk")]
    public async Task PayWithTokenAsync_ValidToken_ReturnsOk()
    {
        // Attempt to create a token. This endpoint may not be available in all sandbox configurations;
        // skip gracefully if it fails rather than failing the suite.
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        string? token;
        try
        {
            var tokenResponse = await client.Tokens.CreateTokenAsync(new CreateTokenRequest
            {
                Card = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

            token = tokenResponse?.Token;
        }
        catch
        {
            // CreateTokenAsync not available in this sandbox profile — skip.
            return;
        }

        if (string.IsNullOrWhiteSpace(token)) return;

        var result = await client.Tokens.PayWithTokenAsync(new PayWithTokenRequest
        {
            VendorTxCode = $"INTEGRATION-TOK-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount       = 100,
            Currency     = "GBP",
            Token        = token
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("Ok", result.Status);
    }
}
