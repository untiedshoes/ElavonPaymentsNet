using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Services;
using Microsoft.Extensions.Logging;

namespace ElavonPaymentsNet;

/// <summary>
/// The main entry point for the ElavonPaymentsNet SDK.
/// </summary>
/// <example>
/// <code>
/// var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
/// {
///     IntegrationKey = "your-key",
///     IntegrationPassword = "your-password",
///     Environment = ElavonEnvironment.Sandbox
/// });
///
/// var result = await client.Transactions.CreateTransactionAsync(new CreatePaymentRequest { ... });
/// </code>
/// </example>
public sealed class ElavonPaymentsClient
{
    /// <summary>Transaction operations: create, authorise, defer, repeat.</summary>
    public IElavonTransactionService Transactions { get; }

    /// <summary>Post-payment operations: capture, refund, void.</summary>
    public IElavonPostPaymentService PostPayments { get; }

    /// <summary>3D Secure challenge flow operations.</summary>
    public IElavonThreeDsService ThreeDs { get; }

    /// <summary>Card tokenisation and token-based payment operations.</summary>
    public IElavonTokensService Tokens { get; }

    /// <summary>Merchant session key and Apple Pay wallet operations.</summary>
    public IElavonWalletsService Wallets { get; }

    /// <summary>Card identifier operations for drop-in UI and server-side flows.</summary>
    public IElavonCardIdentifiersService CardIdentifiers { get; }

    /// <summary>Instruction operations for managing transaction lifecycle states.</summary>
    public IElavonInstructionsService Instructions { get; }

    /// <summary>
    /// Initialises the client using the supplied <paramref name="options"/>
    /// and an internally managed <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// For production use, prefer the overload that accepts an <see cref="HttpClient"/>
    /// sourced from <c>IHttpClientFactory</c> via dependency injection.
    /// </remarks>
    /// <param name="options">Client configuration including credentials and environment.</param>
    public ElavonPaymentsClient(ElavonPaymentsClientOptions options)
        : this(options, CreateHttpClient(options))
    { }

    /// <summary>
    /// Initialises the client using the supplied <paramref name="options"/>
    /// and an internally managed <see cref="HttpClient"/>, with logging sourced
    /// from the provided <paramref name="loggerFactory"/>.
    /// </summary>
    /// <param name="options">Client configuration including credentials and environment.</param>
    /// <param name="loggerFactory">The logger factory used to create the SDK HTTP logger.</param>
    public ElavonPaymentsClient(ElavonPaymentsClientOptions options, ILoggerFactory loggerFactory)
        : this(options, CreateHttpClient(options, loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))))
    { }

    private static HttpClient CreateHttpClient(ElavonPaymentsClientOptions options)
        => CreateHttpClient(options, loggerFactory: null);

    private static HttpClient CreateHttpClient(ElavonPaymentsClientOptions options, ILoggerFactory? loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(options);

        var resilienceHandler = new ElavonResilienceHandler(options.MaxRetryAttempts)
        {
            InnerHandler = new HttpClientHandler()
        };
        var authHandler = new ElavonAuthenticationHandler(options.IntegrationKey, options.IntegrationPassword)
        {
            InnerHandler = resilienceHandler
        };
        var loggingHandler = new ElavonLoggingHandler(loggerFactory)
        {
            InnerHandler = authHandler
        };
        return new HttpClient(loggingHandler)
        {
            BaseAddress = new Uri(options.ApiBaseUrl),
            Timeout = options.Timeout
        };
    }

    /// <summary>
    /// Initialises the client with a provided <see cref="HttpClient"/>.
    /// Use this overload when managing the client lifetime via <c>IHttpClientFactory</c>.
    /// </summary>
    /// <param name="options">Client configuration including credentials and environment.</param>
    /// <param name="httpClient">The preconfigured HTTP client used for all SDK requests.</param>
    public ElavonPaymentsClient(ElavonPaymentsClientOptions options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);

        if (string.IsNullOrWhiteSpace(options.IntegrationKey))
            throw new ArgumentException("IntegrationKey is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.IntegrationPassword))
            throw new ArgumentException("IntegrationPassword is required.", nameof(options));

        var apiClient = new ElavonApiClient(httpClient);

        Transactions   = new ElavonTransactionService(apiClient);
        PostPayments   = new ElavonPostPaymentService(apiClient);
        ThreeDs        = new ElavonThreeDsService(apiClient);
        Tokens         = new ElavonTokensService(apiClient);
        Wallets        = new ElavonWalletsService(apiClient);
        CardIdentifiers = new ElavonCardIdentifiersService(apiClient);
        Instructions   = new ElavonInstructionsService(apiClient);
    }
}
