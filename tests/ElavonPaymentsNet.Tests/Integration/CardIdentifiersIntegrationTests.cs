namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Card Identifiers service against the real Elavon sandbox API.
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
        Assert.Matches(
            @"(?i)^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
            response.CardIdentifier!);
        Assert.False(string.IsNullOrWhiteSpace(response.Expiry));
        Assert.False(string.IsNullOrWhiteSpace(response.CardType));
    }
}
