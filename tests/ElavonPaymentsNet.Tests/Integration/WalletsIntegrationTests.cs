using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Wallets service against the real Elavon sandbox API.
/// Covers merchant session key creation and validation.
/// </summary>
[Trait("Category", "Integration")]
public sealed class WalletsIntegrationTests
{
    /// <summary>
    /// Verifies that a merchant session key can be created using the sandbox Basic profile.
    /// </summary>
    [Fact(DisplayName = "CreateMerchantSessionKeyAsync ReturnsSessionKeyAndExpiry")]
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
    [Fact(DisplayName = "ValidateMerchantSessionKeyAsync FreshKey ReturnsValid")]
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
}
