using Polly;
using Polly.Retry;

namespace ElavonPaymentsNet.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that applies a conservative retry policy
/// to outbound HTTP requests made by the Elavon SDK.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Safety contract:</strong> Only GET requests are eligible for automatic retry.
/// All POST requests pass through without retry, regardless of the response status or
/// exception type. This prevents duplicate financial operations (payments, captures,
/// refunds, instructions) caused by silent server-side processing on a connection that
/// failed before the response arrived.
/// </para>
/// <para>
/// Retry is triggered by:
/// <list type="bullet">
///   <item>5xx server error responses</item>
///   <item><see cref="HttpRequestException"/> (network failure, DNS error, connection reset)</item>
///   <item><see cref="TaskCanceledException"/> (HTTP timeout)</item>
/// </list>
/// 4xx client errors are never retried — they indicate a request problem that will not
/// resolve with repetition.
/// </para>
/// <para>
/// Backoff: exponential with jitter, base interval of 1 second, maximum of
/// <c>maxRetryAttempts</c> additional attempts after the first failure.
/// </para>
/// </remarks>
internal sealed class ElavonResilienceHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    /// <param name="maxRetryAttempts">
    /// Maximum number of retry attempts after the initial failure.
    /// Must be between 1 and 10.
    /// </param>
    internal ElavonResilienceHandler(int maxRetryAttempts)
    {
        if (maxRetryAttempts < 1 || maxRetryAttempts > 10)
            throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts), "Must be between 1 and 10.");

        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                // Transient conditions that warrant a retry on GET requests
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(response => (int)response.StatusCode >= 500),

                MaxRetryAttempts = maxRetryAttempts,

                // Exponential backoff: ~1 s, ~2 s, ~4 s, … with per-attempt jitter
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true
            })
            .Build();
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // POST, PUT, PATCH, DELETE — all financial mutating operations.
        // Never retry: a server-side effect may have occurred before the connection dropped.
        if (request.Method != HttpMethod.Get)
            return base.SendAsync(request, cancellationToken);

        // GET — safe to retry. The pipeline respects the caller's CancellationToken,
        // so a user-initiated cancellation is not retried.
        return _pipeline.ExecuteAsync(
            ct => new ValueTask<HttpResponseMessage>(base.SendAsync(request, ct)),
            cancellationToken).AsTask();
    }
}
