namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for creating a merchant session key.</summary>
public sealed class MerchantSessionRequest
{
    /// <summary>
    /// Optional vendor name for profiles that require explicit merchant identification
    /// when creating merchant session keys.
    /// </summary>
    public string? VendorName { get; init; }

    /// <summary>Optional: supply an existing key to renew or check.</summary>
    public string? MerchantSessionKey { get; init; }
}
