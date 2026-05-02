namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for completing a 3D Secure v2 challenge.</summary>
public sealed class Complete3DsRequest
{
    /// <summary>
    /// The Base64-encoded challenge response (CRes) returned by the card issuer's ACS.
    /// The ACS posts this to your notification URL in a field named <c>cres</c> (lowercase);
    /// it must be forwarded to Opayo as <c>cRes</c> (capital R).
    /// </summary>
    public required string CRes { get; init; }
}
