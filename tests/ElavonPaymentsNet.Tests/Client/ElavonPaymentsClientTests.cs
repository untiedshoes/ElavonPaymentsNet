namespace ElavonPaymentsNet.Tests.Client;

public class ElavonPaymentsClientTests
{
    [Fact]
    public void Constructor_WithValidOptions_ExposesAllServices()
    {
        var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
        {
            IntegrationKey = "test-key",
            IntegrationPassword = "test-password"
        });

        Assert.NotNull(client.Transactions);
        Assert.NotNull(client.PostPayments);
        Assert.NotNull(client.ThreeDs);
        Assert.NotNull(client.Tokens);
        Assert.NotNull(client.Wallets);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("  ", "password")]
    public void Constructor_WithMissingIntegrationKey_Throws(string key, string password)
    {
        Assert.Throws<ArgumentException>(() => new ElavonPaymentsClient(
            new ElavonPaymentsClientOptions
            {
                IntegrationKey = key,
                IntegrationPassword = password
            }));
    }

    [Theory]
    [InlineData("key", "")]
    [InlineData("key", "  ")]
    public void Constructor_WithMissingIntegrationPassword_Throws(string key, string password)
    {
        Assert.Throws<ArgumentException>(() => new ElavonPaymentsClient(
            new ElavonPaymentsClientOptions
            {
                IntegrationKey = key,
                IntegrationPassword = password
            }));
    }

    [Fact]
    public void Options_DefaultEnvironment_IsSandbox()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "k",
            IntegrationPassword = "p"
        };

        Assert.Equal(Http.ElavonEnvironment.Sandbox, options.Environment);
        Assert.Contains("sandbox", options.ApiBaseUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Options_LiveEnvironment_UsesLiveBaseUrl()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "k",
            IntegrationPassword = "p",
            Environment = Http.ElavonEnvironment.Live
        };

        Assert.Contains("live", options.ApiBaseUrl, StringComparison.OrdinalIgnoreCase);
    }
}
