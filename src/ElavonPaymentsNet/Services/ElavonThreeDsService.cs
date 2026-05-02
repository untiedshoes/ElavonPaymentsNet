using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides 3D Secure v2 challenge flow operations.
/// Access via <c>client.ThreeDs</c>.
/// </summary>
internal sealed class ElavonThreeDsService : IElavonThreeDsService
{
    private readonly ElavonApiClient _api;

    internal ElavonThreeDsService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Completes a 3D Secure v2 challenge by submitting the cRes received from the card issuer's ACS.
    /// The <c>acsUrl</c> and <c>cReq</c> needed to initiate the challenge are returned directly on
    /// the original <see cref="PaymentResponse"/> when <c>Status</c> is "3DAuth".
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID.</param>
    /// <param name="request">The completion request containing the cRes value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<Complete3DsResponse> Complete3DsAsync(string transactionId, Complete3DsRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<Complete3DsRequest, Complete3DsResponse>(
            HttpMethod.Post, ElavonApiRoutes.Transaction3DsChallenge(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
