using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines transaction creation operations.</summary>
public interface IElavonTransactionService
{
    /// <summary>Creates a transaction. The <see cref="CreateTransactionRequest.TransactionType"/> determines the operation.</summary>
    Task<PaymentResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves an existing transaction by its Elavon-assigned transaction ID.</summary>
    /// <param name="transactionId">The Elavon-assigned transaction ID to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<PaymentResponse> RetrieveTransactionAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconciles an unknown POST outcome using <paramref name="vendorTxCode"/> and a caller-supplied
    /// lookup function that resolves the Elavon transaction ID in your own persistence layer.
    /// </summary>
    /// <param name="vendorTxCode">The stable vendor transaction code used on the original POST attempt.</param>
    /// <param name="resolveTransactionIdByVendorTxCode">
    /// A function that returns the Elavon transaction ID associated with the supplied vendorTxCode,
    /// or <see langword="null"/> if no mapping is known yet.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The retrieved transaction when a mapping is found; otherwise <see langword="null"/>.
    /// </returns>
    Task<PaymentResponse?> ResolveUnknownTransactionAsync(
        string vendorTxCode,
        Func<string, CancellationToken, Task<string?>> resolveTransactionIdByVendorTxCode,
        CancellationToken cancellationToken = default);
}
