using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Mapping;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using ElavonPaymentsNet.Validation;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides operations for creating and managing payment transactions.
/// Access via <c>client.Transactions</c>.
/// </summary>
internal sealed class ElavonTransactionService : IElavonTransactionService
{
    private readonly ElavonApiClient _api;

    internal ElavonTransactionService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Creates a transaction. The <see cref="CreateTransactionRequest.TransactionType"/>
    /// determines whether it is a payment, deferred, authenticate, repeat, refund, or authorise transaction.
    /// </summary>
    public async Task<PaymentResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto = RequestMapper.ToDto(request);
        return await _api.SendAsync<CreateTransactionRequestDto, PaymentResponse>(
            HttpMethod.Post, ElavonApiRoutes.Transactions, dto, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves an existing transaction by its Elavon-assigned transaction ID.
    /// </summary>
    public async Task<PaymentResponse> RetrieveTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(transactionId, nameof(transactionId));

        return await _api.SendAsync<PaymentResponse>(
            HttpMethod.Get, ElavonApiRoutes.TransactionById(transactionId), null, cancellationToken)
            .ConfigureAwait(false);
    }
}
