namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for completing a 3D Secure challenge flow.</summary>
public sealed class Complete3DsRequest
{
    /// <summary>The challenge result string (CRes) returned by the card issuer's ACS.</summary>
    public required string Cres { get; init; }
}
