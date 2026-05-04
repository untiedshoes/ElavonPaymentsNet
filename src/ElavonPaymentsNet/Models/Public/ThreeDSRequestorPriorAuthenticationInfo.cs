namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Details of prior cardholder authentication that may be reused during EMV 3DS evaluation.
/// </summary>
public sealed class ThreeDSRequestorPriorAuthenticationInfo
{
    /// <summary>
    /// Prior authentication method indicator code.
    /// </summary>
    public string? ThreeDSReqPriorAuthMethod { get; init; }

    /// <summary>
    /// Timestamp of prior authentication in YYYYMMDDHHMM format.
    /// </summary>
    public string? ThreeDSReqPriorAuthTimestamp { get; init; }

    /// <summary>
    /// Reference data for the prior authentication event.
    /// </summary>
    public string? ThreeDSReqPriorRef { get; init; }
}
