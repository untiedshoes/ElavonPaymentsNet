using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines merchant session key and Apple Pay wallet operations.</summary>
public interface IElavonWalletsService
{
    /// <summary>Creates a new merchant session key for use with drop-in card fields.</summary>
    Task<MerchantSessionResponse> CreateMerchantSessionKeyAsync(MerchantSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Validates whether an existing merchant session key is still active.</summary>
    Task<MerchantSessionValidationResponse> ValidateMerchantSessionKeyAsync(MerchantSessionValidationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Obtains an Apple Pay merchant session from Elavon, to be passed to <c>completeMerchantValidation</c> in the browser.</summary>
    Task<ApplePaySessionResponse> CreateApplePaySessionAsync(ApplePaySessionRequest request, CancellationToken cancellationToken = default);
}
