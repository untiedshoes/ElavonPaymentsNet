using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines post-payment operations: capture, refund, and void.</summary>
public interface IElavonPostPaymentService
{
    /// <summary>Captures a previously deferred or authorised payment.</summary>
    Task<PostPaymentResponse> CaptureTransactionAsync(string transactionId, CapturePaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Refunds a previously settled payment, either fully or partially.</summary>
    Task<PostPaymentResponse> RefundTransactionAsync(string transactionId, RefundPaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Voids a payment that has not yet been settled (same-day cancellation).</summary>
    Task<PostPaymentResponse> VoidTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
}
