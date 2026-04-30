using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Mapping;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides operations for creating and managing payment transactions.
/// Access via <c>client.Transactions</c>.
/// </summary>
public class ElavonTransactionService : IElavonTransactionService
{
    private readonly ElavonApiClient _api;

    internal ElavonTransactionService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Creates a payment transaction. The <see cref="CreateTransactionRequest.TransactionType"/>
    /// determines whether it is a standard payment, authorisation, deferred, or repeat transaction.
    /// </summary>
    public async Task<PaymentResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var dto = RequestMapper.ToDto(request);
        return await _api.SendAsync<CreateTransactionRequestDto, PaymentResponse>(
            HttpMethod.Post, "/transactions", dto, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
