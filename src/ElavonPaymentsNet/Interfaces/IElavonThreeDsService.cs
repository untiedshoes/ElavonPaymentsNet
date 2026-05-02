using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines 3D Secure v2 challenge flow operations.</summary>
public interface IElavonThreeDsService
{
    /// <summary>
    /// Completes a 3D Secure v2 challenge by submitting the cRes received from the card issuer's ACS.
    /// Call this after the customer completes the challenge at the <c>acsUrl</c> returned by the
    /// original transaction response and your notification URL receives the <c>cres</c> POST.
    /// </summary>
    Task<Complete3DsResponse> Complete3DsAsync(string transactionId, Complete3DsRequest request, CancellationToken cancellationToken = default);
}
