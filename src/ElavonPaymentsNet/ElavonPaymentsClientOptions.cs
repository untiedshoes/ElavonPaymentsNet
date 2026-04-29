using ElavonPaymentsNet.Http;

namespace ElavonPaymentsNet;

/// <summary>
/// Configuration options for <see cref="ElavonPaymentsClient"/>.
/// </summary>
public sealed class ElavonPaymentsClientOptions
{
    /// <summary>
    /// Your Opayo integration key (vendor name / username).
    /// Required.
    /// </summary>
    public required string IntegrationKey { get; init; }

    /// <summary>
    /// Your Opayo integration password.
    /// Required.
    /// </summary>
    public required string IntegrationPassword { get; init; }

    /// <summary>
    /// The environment to target. Defaults to <see cref="ElavonEnvironment.Sandbox"/>.
    /// </summary>
    public ElavonEnvironment Environment { get; init; } = ElavonEnvironment.Sandbox;

    /// <summary>
    /// Optional HTTP request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts after a transient failure on GET requests.
    /// Only GET requests are retried — POST requests are never retried to prevent
    /// duplicate financial operations. Defaults to 3. Must be between 1 and 10.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    internal string ApiBaseUrl => Environment switch
    {
        ElavonEnvironment.Live => "https://live.opayo.eu.elavon.com/api/v1",
        _ => "https://sandbox.opayo.eu.elavon.com/api/v1"
    };
}
