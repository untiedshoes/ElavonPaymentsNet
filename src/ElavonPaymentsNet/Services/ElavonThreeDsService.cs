using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides 3D Secure (3DS) challenge flow operations.
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
    /// Initialises a 3D Secure challenge for a transaction that returned a
    /// "3DAuth" or "ChallengeRequired" status.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID requiring 3DS.</param>
    /// <param name="request">The 3DS initialise request containing the notification URL.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<Initialise3DsResponse> Initialise3DsAsync(string transactionId, Initialise3DsRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<Initialise3DsRequest, Initialise3DsResponse>(
            HttpMethod.Post, ElavonApiRoutes.Transaction3Ds(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Completes a 3D Secure challenge flow after receiving the CRes from the card issuer.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID.</param>
    /// <param name="request">The completion request containing the CRes value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<Complete3DsResponse> Complete3DsAsync(string transactionId, Complete3DsRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<Complete3DsRequest, Complete3DsResponse>(
            HttpMethod.Post, ElavonApiRoutes.Transaction3DsComplete(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
