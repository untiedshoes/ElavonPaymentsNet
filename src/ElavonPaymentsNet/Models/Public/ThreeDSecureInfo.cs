namespace ElavonPaymentsNet.Models.Public;

/// <summary>3D Secure result returned as part of a payment response.</summary>
public sealed class ThreeDSecureInfo
{
    /// <summary>The 3D Secure status, e.g. "Authenticated", "NotRequired", "ChallengeRequired".</summary>
    public string? Status { get; init; }
}
