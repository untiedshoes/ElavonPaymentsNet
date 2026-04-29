namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for creating a merchant session key.</summary>
public sealed class MerchantSessionRequest
{
    /// <summary>Optional: supply an existing key to renew or check.</summary>
    public string? MerchantSessionKey { get; init; }
}
