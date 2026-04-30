using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines card identifier operations for use with drop-in UI flows.</summary>
public interface IElavonCardIdentifiersService
{
    /// <summary>
    /// Creates a card identifier against a merchant session key.
    /// The card identifier can then be used in place of card details in a payment request.
    /// </summary>
    /// <param name="merchantSessionKey">A valid merchant session key obtained from <c>client.Wallets.CreateMerchantSessionKeyAsync</c>.</param>
    /// <param name="request">The card details to tokenise.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<CreateCardIdentifierResponse> CreateCardIdentifierAsync(string merchantSessionKey, CreateCardIdentifierRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a security code (CVV/CV2) to an existing card identifier.
    /// Required when the card identifier was created without a security code.
    /// </summary>
    /// <param name="cardIdentifier">The card identifier token to update.</param>
    /// <param name="request">The security code to link.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LinkCardIdentifierAsync(string cardIdentifier, LinkCardIdentifierRequest request, CancellationToken cancellationToken = default);
}
