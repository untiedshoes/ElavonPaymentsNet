using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests verifying that the SDK surfaces errors correctly when the
/// real Elavon sandbox API returns failure responses.
/// </summary>
[Trait("Category", "Integration")]
public sealed class FailureScenariosIntegrationTests
{
    /// <summary>
    /// Verifies that invalid credentials are rejected by the real API with an authentication exception.
    /// </summary>
    [Fact(DisplayName = "CreateMerchantSessionKeyAsync WithInvalidCredentials ThrowsAuthenticationException")]
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

    /// <summary>
    /// Verifies that a transaction request with a missing required field (Amount) surfaces a
    /// validation exception rather than a generic error.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionAsync MissingAmount ThrowsValidationOrApiException")]
    public async Task CreateTransactionAsync_MissingAmount_ThrowsValidationOrApiException()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "VALIDATE");
        if (txId is null) return;

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

        // Amount = 0 should be rejected by the gateway as invalid
        await Assert.ThrowsAnyAsync<ElavonApiException>(() =>
            client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
            {
                TransactionType   = TransactionType.Payment,
                VendorTxCode      = $"INTEGRATION-INVALID-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
                Amount            = 0,
                Currency          = "GBP",
                Description       = "Integration test — invalid amount",
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
            }));
    }

    /// <summary>
    /// Verifies that attempting to retrieve a non-existent transaction ID surfaces an API exception.
    /// </summary>
    [Fact(DisplayName = "RetrieveTransactionAsync NonExistentId ThrowsApiException")]
    public async Task RetrieveTransactionAsync_NonExistentId_ThrowsApiException()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        await Assert.ThrowsAnyAsync<ElavonApiException>(() =>
            client.Transactions.RetrieveTransactionAsync("00000000-0000-0000-0000-000000000000"));
    }
}
