namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>
/// Response returned after validating a merchant session key.
/// The live API returns the same payload shape as <see cref="MerchantSessionResponse"/>.
/// </summary>
public sealed class MerchantSessionValidationResponse
{
    /// <summary>The merchant session key that was validated.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>The UTC expiry date/time of the session key.</summary>
    public DateTimeOffset? Expiry { get; init; }

    /// <summary>
    /// Convenience property — <see langword="true"/> when the key and expiry
    /// were returned by the API (i.e. the key is still active).
    /// </summary>
    public bool Valid => !string.IsNullOrWhiteSpace(MerchantSessionKey) && Expiry.HasValue;
}
