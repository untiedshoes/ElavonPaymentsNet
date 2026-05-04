namespace ElavonPaymentsNet.Models.Public;

/// <summary>iDEAL payment details.</summary>
public sealed class IdealPaymentMethod
{
    /// <summary>Language code used for the payment journey.</summary>
    public string? LanguageCode { get; init; }

    /// <summary>Merchant session key associated with the payment.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>Callback URL for post-authorization return.</summary>
    public string? CallbackUrl { get; init; }
}
