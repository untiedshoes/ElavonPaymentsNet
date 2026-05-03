using ElavonHttp = ElavonPaymentsNet.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElavonPaymentsNet.Tests.Client;

/// <summary>
/// Unit tests covering public client construction and option-derived base URL behavior.
/// </summary>
[Trait("Category", "Unit")]
public class ElavonPaymentsClientTests
{
    /// <summary>
    /// Verifies that constructing the client with valid options exposes every service group.
    /// </summary>
    [Fact(DisplayName = "Constructor WithValidOptions ExposesAllServices")]
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
        Assert.NotNull(client.CardIdentifiers);
        Assert.NotNull(client.Instructions);
    }

    /// <summary>
    /// Verifies that the logger-factory overload also exposes every service group.
    /// </summary>
    [Fact(DisplayName = "Constructor WithLoggerFactory ExposesAllServices")]
    public void Constructor_WithLoggerFactory_ExposesAllServices()
    {
        var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
        {
            IntegrationKey = "test-key",
            IntegrationPassword = "test-password"
        }, NullLoggerFactory.Instance);

        Assert.NotNull(client.Transactions);
        Assert.NotNull(client.PostPayments);
        Assert.NotNull(client.ThreeDs);
        Assert.NotNull(client.Tokens);
        Assert.NotNull(client.Wallets);
        Assert.NotNull(client.CardIdentifiers);
        Assert.NotNull(client.Instructions);
    }

    /// <summary>
    /// Verifies that a null options instance is rejected immediately.
    /// </summary>
    [Fact(DisplayName = "Constructor WithNullOptions ThrowsArgumentNullException")]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ElavonPaymentsClient((ElavonPaymentsClientOptions)null!));
    }

    /// <summary>
    /// Verifies that a null logger factory is rejected immediately.
    /// </summary>
    [Fact(DisplayName = "Constructor WithNullLoggerFactory ThrowsArgumentNullException")]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "test-key",
            IntegrationPassword = "test-password"
        };

        Assert.Throws<ArgumentNullException>(() => new ElavonPaymentsClient(options, (Microsoft.Extensions.Logging.ILoggerFactory)null!));
    }

    /// <summary>
    /// Verifies that a null HTTP client is rejected immediately.
    /// </summary>
    [Fact(DisplayName = "Constructor WithNullHttpClient ThrowsArgumentNullException")]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "test-key",
            IntegrationPassword = "test-password"
        };

        Assert.Throws<ArgumentNullException>(() => new ElavonPaymentsClient(options, (HttpClient)null!));
    }

    /// <summary>
    /// Verifies that a blank integration key is rejected.
    /// </summary>
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

            /// <summary>
            /// Verifies that a blank integration password is rejected.
            /// </summary>
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

            /// <summary>
            /// Verifies that the default environment resolves to the sandbox base URL.
            /// </summary>
    [Fact(DisplayName = "Options DefaultEnvironment IsSandbox")]
    public void Options_DefaultEnvironment_IsSandbox()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "k",
            IntegrationPassword = "p"
        };

        Assert.Equal(ElavonHttp.ElavonEnvironment.Sandbox, options.Environment);
        Assert.Contains("sandbox", options.ApiBaseUrl, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that the live environment resolves to the live base URL.
    /// </summary>
    [Fact(DisplayName = "Options LiveEnvironment UsesLiveBaseUrl")]
    public void Options_LiveEnvironment_UsesLiveBaseUrl()
    {
        var options = new ElavonPaymentsClientOptions
        {
            IntegrationKey = "k",
            IntegrationPassword = "p",
            Environment = ElavonHttp.ElavonEnvironment.Live
        };

        Assert.Contains("live", options.ApiBaseUrl, StringComparison.OrdinalIgnoreCase);
    }
}
