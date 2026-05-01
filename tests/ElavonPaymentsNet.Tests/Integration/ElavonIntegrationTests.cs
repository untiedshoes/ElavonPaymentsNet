using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
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
    private const string SkipMessage = "Set ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_SAFE_TRANSACTION_ID to run happy-path integration tests.";

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
    /// Checks whether all required environment variables for happy-path integration tests are present.
    /// </summary>
    private static bool HasIntegrationEnvironment()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_KEY"))
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD"))
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
