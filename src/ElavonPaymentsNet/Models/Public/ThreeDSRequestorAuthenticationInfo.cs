namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Details about how the 3DS requestor authenticated the cardholder.
/// </summary>
public sealed class ThreeDSRequestorAuthenticationInfo
{
    /// <summary>
    /// Authentication data provided by the 3DS requestor.
    /// </summary>
    public string? ThreeDSReqAuthData { get; init; }

    /// <summary>
    /// 3DS requestor authentication method indicator.
    /// </summary>
    public string? ThreeDSReqAuthMethod { get; init; }

    /// <summary>
    /// Timestamp of authentication in YYYYMMDDHHMM format.
    /// </summary>
    public string? ThreeDSReqAuthTimestamp { get; init; }
}
