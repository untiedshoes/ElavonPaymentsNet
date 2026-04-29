using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Services;

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
    public ElavonPaymentsClient(ElavonPaymentsClientOptions options)
        : this(options, new HttpClient { BaseAddress = new Uri(options.ApiBaseUrl), Timeout = options.Timeout })
    { }

    /// <summary>
    /// Initialises the client with a provided <see cref="HttpClient"/>.
    /// Use this overload when managing the client lifetime via <c>IHttpClientFactory</c>.
    /// </summary>
    public ElavonPaymentsClient(ElavonPaymentsClientOptions options, HttpClient httpClient)
    {
        if (string.IsNullOrWhiteSpace(options.IntegrationKey))
            throw new ArgumentException("IntegrationKey is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.IntegrationPassword))
            throw new ArgumentException("IntegrationPassword is required.", nameof(options));

        var api = new ElavonApiClient(httpClient);

        Transactions = new ElavonTransactionService(api, options);
        PostPayments = new ElavonPostPaymentService(api, options);
        ThreeDs = new ElavonThreeDsService(api, options);
        Tokens = new ElavonTokensService(api, options);
        Wallets = new ElavonWalletsService(api, options);
        CardIdentifiers = new ElavonCardIdentifiersService(api, options);
        Instructions = new ElavonInstructionsService(api, options);
    }
}
