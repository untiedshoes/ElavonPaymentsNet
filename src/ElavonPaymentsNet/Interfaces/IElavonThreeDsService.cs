using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines 3D Secure challenge flow operations.</summary>
public interface IElavonThreeDsService
{
    /// <summary>Initialises a 3D Secure challenge for a transaction requiring 3DS.</summary>
    Task<Initialise3DsResponse> Initialise3DsAsync(string transactionId, Initialise3DsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Completes a 3D Secure challenge flow after receiving the CRes from the card issuer.</summary>
    Task<Complete3DsResponse> Complete3DsAsync(string transactionId, Complete3DsRequest request, CancellationToken cancellationToken = default);
}
