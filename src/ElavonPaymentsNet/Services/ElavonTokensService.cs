using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Mapping;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides operations for creating and using stored card tokens.
/// Access via <c>client.Tokens</c>.
/// </summary>
internal sealed class ElavonTokensService : IElavonTokensService
{
    private readonly ElavonApiClient _api;

    internal ElavonTokensService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Tokenises a card for future payments without processing a transaction.
    /// </summary>
    public async Task<CreateTokenResponse> CreateTokenAsync(CreateTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<CreateTokenRequest, CreateTokenResponse>(
            HttpMethod.Post, ElavonApiRoutes.Token, request, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Processes a payment using a previously stored card token.
    /// </summary>
    public async Task<PaymentResponse> PayWithTokenAsync(PayWithTokenRequest request, CancellationToken cancellationToken = default)
    {
        var dto = RequestMapper.ToDto(request);
        return await _api.SendAsync<PayWithTokenRequestDto, PaymentResponse>(
            HttpMethod.Post, ElavonApiRoutes.Transactions, dto, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
