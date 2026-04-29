namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after initialising a 3D Secure challenge.</summary>
public sealed class Initialise3DsResponse
{
    /// <summary>The 3DS status, e.g. "ChallengeRequired".</summary>
    public string? Status { get; init; }

    /// <summary>The URL of the card issuer's access control server (ACS).</summary>
    public string? AcsUrl { get; init; }

    /// <summary>The challenge request data to POST to the ACS.</summary>
    public string? CReq { get; init; }
}
