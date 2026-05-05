namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Card Identifiers service against the real Elavon sandbox API.
/// Covers creating, linking, and removing card identifiers.
/// </summary>
[Trait("Category", "Integration")]
public sealed class CardIdentifiersIntegrationTests
{
    /// <summary>
    /// Verifies that a card identifier can be created from a fresh merchant session key.
    /// </summary>
    [Fact(DisplayName = "CreateCardIdentifierAsync ReturnsIdentifierExpiryAndCardType")]
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

    /// <summary>
    /// Verifies that a security code can be linked to an existing card identifier.
    /// </summary>
    [Fact(DisplayName = "LinkCardIdentifierAsync ValidIdentifier CompletesWithoutError")]
    public async Task LinkCardIdentifierAsync_ValidIdentifier_CompletesWithoutError()
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

        // LinkCardIdentifierAsync returns void — completing without exception is the assertion.
        await client.CardIdentifiers.LinkCardIdentifierAsync(
            cardId.CardIdentifier!,
            new LinkCardIdentifierRequest { SecurityCode = "123" });
    }

    /// <summary>
    /// Verifies that a card identifier can be removed without error.
    /// </summary>
    [Fact(DisplayName = "RemoveCardIdentifierAsync ValidIdentifier CompletesWithoutError")]
    public async Task RemoveCardIdentifierAsync_ValidIdentifier_CompletesWithoutError()
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

        // RemoveCardIdentifierAsync returns void — completing without exception is the assertion.
        await client.CardIdentifiers.RemoveCardIdentifierAsync(cardId.CardIdentifier!);
    }
}
