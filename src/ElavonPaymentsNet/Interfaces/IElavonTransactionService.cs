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
}
