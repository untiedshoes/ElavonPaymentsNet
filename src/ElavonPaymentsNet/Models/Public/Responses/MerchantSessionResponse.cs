namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after creating or renewing a merchant session key.</summary>
public sealed class MerchantSessionResponse
{
    /// <summary>The merchant session key.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>The UTC expiry date/time of the session key.</summary>
    public DateTimeOffset? Expiry { get; init; }
}
