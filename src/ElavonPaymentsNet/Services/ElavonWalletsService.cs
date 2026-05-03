using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides merchant session key and Apple Pay wallet operations.
/// Access via <c>client.Wallets</c>.
/// </summary>
internal sealed class ElavonWalletsService : IElavonWalletsService
{
    private readonly ElavonApiClient _api;

    internal ElavonWalletsService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Creates a new merchant session key for use with drop-in card fields.
    /// </summary>
    public async Task<MerchantSessionResponse> CreateMerchantSessionKeyAsync(MerchantSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _api.SendAsync<MerchantSessionRequest, MerchantSessionResponse>(
            HttpMethod.Post, ElavonApiRoutes.MerchantSessionKeys, request, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Validates whether an existing merchant session key is still active.
    /// </summary>
    public async Task<MerchantSessionValidationResponse> ValidateMerchantSessionKeyAsync(MerchantSessionValidationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _api.SendAsync<MerchantSessionValidationResponse>(
            HttpMethod.Get, ElavonApiRoutes.MerchantSessionKey(request.MerchantSessionKey), null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Obtains an Apple Pay merchant session from Elavon, to be passed to
    /// completeMerchantValidation in the browser.
    /// </summary>
    public async Task<ApplePaySessionResponse> CreateApplePaySessionAsync(ApplePaySessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _api.SendAsync<ApplePaySessionRequest, ApplePaySessionResponse>(
            HttpMethod.Post, ElavonApiRoutes.ApplePaySession, request, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
