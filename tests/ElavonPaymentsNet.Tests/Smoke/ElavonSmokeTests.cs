using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Tests.Integration;

namespace ElavonPaymentsNet.Tests.Smoke;

/// <summary>
/// Minimal fast smoke suite for CI confidence:
/// - SDK bootstraps with known sandbox credentials.
/// - A safe wallet flow call succeeds end-to-end (MSK create + validate).
/// </summary>
[Trait("Category", "Smoke")]
public sealed class ElavonSmokeTests
{
    [Fact(DisplayName = "Client bootstraps and exposes service groups")]
    public void Client_Bootstraps_And_Exposes_ServiceGroups()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        Assert.NotNull(client.Transactions);
        Assert.NotNull(client.PostPayments);
        Assert.NotNull(client.Instructions);
        Assert.NotNull(client.ThreeDs);
        Assert.NotNull(client.Tokens);
        Assert.NotNull(client.Wallets);
        Assert.NotNull(client.CardIdentifiers);
    }

    [Fact(DisplayName = "Create and validate merchant session key succeeds")]
    public async Task Wallets_CreateAndValidateMerchantSessionKey_Succeeds()
    {
        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = SandboxCredentials.BasicVendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var validation = await client.Wallets.ValidateMerchantSessionKeyAsync(
            new MerchantSessionValidationRequest { MerchantSessionKey = session.MerchantSessionKey! });

        Assert.NotNull(validation);
        Assert.True(validation.Valid);
    }
}
