namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for initialising a 3D Secure challenge flow.</summary>
public sealed class Initialise3DsRequest
{
    /// <summary>The URL to which the card issuer will POST the challenge result.</summary>
    public required string NotificationUrl { get; init; }
}
