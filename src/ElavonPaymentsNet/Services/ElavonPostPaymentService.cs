using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using ElavonPaymentsNet.Validation;

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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<PostPaymentResponse> CaptureTransactionAsync(string transactionId, CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.NotNullOrWhiteSpace(transactionId, nameof(transactionId));

        _ = await _api.SendAsync<InstructionRequest, InstructionResponse>(HttpMethod.Post, ElavonApiRoutes.TransactionInstructions(transactionId),
            new InstructionRequest
            {
                InstructionType = InstructionType.Release,
                Amount = request.Amount
            },
            null,
            cancellationToken).ConfigureAwait(false);

        return new PostPaymentResponse
        {
            TransactionId = transactionId,
            Status = "Ok"
        };
    }

    /// <summary>
    /// Refunds a previously settled payment, either fully or partially.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to refund.</param>
    /// <param name="request">The refund request specifying the amount.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<PostPaymentResponse> RefundTransactionAsync(string transactionId, RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.NotNullOrWhiteSpace(transactionId, nameof(transactionId));

        var response = await _api.SendAsync<CreateTransactionRequestDto, PaymentResponse>(HttpMethod.Post, ElavonApiRoutes.Transactions,
            new CreateTransactionRequestDto
            {
                TransactionType = "Refund",
                VendorTxCode = request.VendorTxCode,
                Amount = request.Amount,
                Description = request.Description,
                ReferenceTransactionId = transactionId
            },
            null,
            cancellationToken).ConfigureAwait(false);

        return new PostPaymentResponse
        {
            TransactionId = response.TransactionId,
            Status = response.Status
        };
    }

    /// <summary>
    /// Voids a payment that has not yet been settled (same-day cancellation).
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to void.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<PostPaymentResponse> VoidTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(transactionId, nameof(transactionId));

        _ = await _api.SendAsync<InstructionRequest, InstructionResponse>(HttpMethod.Post, ElavonApiRoutes.TransactionInstructions(transactionId),
            new InstructionRequest
            {
                InstructionType = InstructionType.Void
            },
            null,
            cancellationToken).ConfigureAwait(false);

        return new PostPaymentResponse
        {
            TransactionId = transactionId,
            Status = "Ok"
        };
    }
}
