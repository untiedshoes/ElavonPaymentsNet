using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines card tokenisation and token-based payment operations.</summary>
public interface IElavonTokensService
{
    /// <summary>Tokenises a card for future payments without processing a transaction.</summary>
    Task<CreateTokenResponse> CreateTokenAsync(CreateTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Processes a payment using a previously stored card token.</summary>
    Task<PaymentResponse> PayWithTokenAsync(PayWithTokenRequest request, CancellationToken cancellationToken = default);
}
