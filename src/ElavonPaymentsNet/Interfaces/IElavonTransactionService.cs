using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines transaction creation operations.</summary>
public interface IElavonTransactionService
{
    /// <summary>Creates a transaction. The <see cref="CreateTransactionRequest.TransactionType"/> determines the operation.</summary>
    Task<PaymentResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
}
