using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using ElavonPaymentsNet.Validation;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides card identifier operations for use with drop-in UI and server-side flows.
/// Access via <c>client.CardIdentifiers</c>.
/// </summary>
internal sealed class ElavonCardIdentifiersService : IElavonCardIdentifiersService
{
    private readonly ElavonApiClient _api;

    internal ElavonCardIdentifiersService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Creates a card identifier against a merchant session key.
    /// Uses Bearer authentication with the supplied merchant session key.
    /// </summary>
    /// <param name="merchantSessionKey">A valid MSK obtained from <c>client.Wallets.CreateMerchantSessionKeyAsync</c>.</param>
    /// <param name="request">The card details to tokenise.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<CreateCardIdentifierResponse> CreateCardIdentifierAsync(string merchantSessionKey,CreateCardIdentifierRequest request,CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.NotNullOrWhiteSpace(merchantSessionKey, nameof(merchantSessionKey));

        return await _api.SendAsync<CreateCardIdentifierRequest, CreateCardIdentifierResponse>(
            HttpMethod.Post, ElavonApiRoutes.CardIdentifiers, request, merchantSessionKey, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Links a security code to an existing card identifier.
    /// Uses Basic authentication.
    /// </summary>
    /// <param name="cardIdentifier">The card identifier token returned by <see cref="CreateCardIdentifierAsync"/>.</param>
    /// <param name="request">The security code to link.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task LinkCardIdentifierAsync(string cardIdentifier,LinkCardIdentifierRequest request,CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.NotNullOrWhiteSpace(cardIdentifier, nameof(cardIdentifier));

        await _api.SendVoidAsync<LinkCardIdentifierRequest>(
            HttpMethod.Post, ElavonApiRoutes.CardIdentifierSecurityCode(cardIdentifier), request, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
