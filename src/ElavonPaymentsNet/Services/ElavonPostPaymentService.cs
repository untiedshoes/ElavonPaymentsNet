using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides post-payment operations: capture, refund, and void.
/// Access via <c>client.PostPayments</c>.
/// </summary>
internal sealed class ElavonPostPaymentService : IElavonPostPaymentService
{
    private readonly ElavonApiClient _api;

    internal ElavonPostPaymentService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Captures a previously deferred or authorised payment.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to capture.</param>
    /// <param name="request">The capture request specifying the amount.</param>
    public async Task<PostPaymentResponse> CaptureTransactionAsync(string transactionId, CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<CapturePaymentRequest, PostPaymentResponse>(
            HttpMethod.Post, ElavonApiRoutes.TransactionCapture(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Refunds a previously settled payment, either fully or partially.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to refund.</param>
    /// <param name="request">The refund request specifying the amount.</param>
    public async Task<PostPaymentResponse> RefundTransactionAsync(string transactionId, RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<RefundPaymentRequest, PostPaymentResponse>(
            HttpMethod.Post, ElavonApiRoutes.TransactionRefund(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Voids a payment that has not yet been settled (same-day cancellation).
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to void.</param>
    public async Task<PostPaymentResponse> VoidTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<PostPaymentResponse>(
            HttpMethod.Post, ElavonApiRoutes.TransactionVoid(transactionId), null, cancellationToken)
            .ConfigureAwait(false);
    }
}
