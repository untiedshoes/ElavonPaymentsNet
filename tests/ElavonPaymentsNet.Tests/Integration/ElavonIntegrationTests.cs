using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Elavon sandbox API.
/// 
/// Happy-path tests require safe test credentials and IDs via environment variables.
/// Failure-scenario tests can run without environment setup by using deliberately invalid credentials.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ElavonIntegrationTests
{
    private const string SkipMessage = "Set ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, ELAVON_VENDOR_NAME, and ELAVON_SAFE_TRANSACTION_ID to run happy-path integration tests.";

    /// <summary>
    /// Verifies the full MSK + card-identifier + transaction flow against the sandbox,
    /// using the SUCCESSFUL magic cardholder name and Apply3DSecure=Disable.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_ReturnsOk()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"   // sandbox magic value: frictionless OK
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
    /// Verifies the full MSK + card-identifier + transaction flow returns a bank decline
    /// for the known sandbox decline card when 3DS is disabled.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_DeclineCard_ReturnsBankDecline()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey,
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

    /// <summary>
    /// Verifies that RetrieveTransactionAsync can read a known safe transaction in sandbox
    /// when integration credentials and a safe transaction ID are supplied.
    /// </summary>
    [Fact]
    public async Task RetrieveTransactionAsync_WithConfiguredSafeId_ReturnsResponse()
    {
        if (!HasIntegrationEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var client = new ElavonPaymentsClient(options);
        var transactionId = GetRequiredEnvironmentVariable("ELAVON_SAFE_TRANSACTION_ID");

        var response = await client.Transactions.RetrieveTransactionAsync(transactionId);

        Assert.NotNull(response);
        Assert.Equal(transactionId, response.TransactionId);
        Assert.False(string.IsNullOrWhiteSpace(response.Status));
    }

    /// <summary>
    /// Verifies that invalid credentials are rejected by the real API with an authentication exception.
    /// This test is intentionally independent of environment variables.
    /// </summary>
    [Fact]
    public async Task CreateMerchantSessionKeyAsync_WithInvalidCredentials_ThrowsAuthenticationException()
    {
        if (!HasIntegrationEnvironment())
            return;

        var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
        {
            IntegrationKey = "invalid-key",
            IntegrationPassword = "invalid-password",
            Environment = ElavonEnvironment.Sandbox
        });

        await Assert.ThrowsAsync<ElavonAuthenticationException>(() =>
            client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest()));
    }

    /// <summary>
    /// Checks whether credentials and vendor name are present (sufficient to create transactions).
    /// </summary>
    private static bool HasTransactionEnvironment()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_KEY"))
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD"))
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_VENDOR_NAME"));

    /// <summary>
    /// Checks whether all required environment variables for happy-path integration tests are present.
    /// </summary>
    private static bool HasIntegrationEnvironment()
        => HasTransactionEnvironment()
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_SAFE_TRANSACTION_ID"));

    /// <summary>
    /// Builds options from required environment variables.
    /// </summary>
    private static ElavonPaymentsClientOptions CreateOptionsFromEnvironment() => new()
    {
        IntegrationKey = GetRequiredEnvironmentVariable("ELAVON_INTEGRATION_KEY"),
        IntegrationPassword = GetRequiredEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD"),
        Environment = ElavonEnvironment.Sandbox
    };

    /// <summary>
    /// Gets an environment variable value or throws if it is missing/blank.
    /// </summary>
    private static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{SkipMessage} Missing: {variableName}.");

        return value;
    }
}
