namespace ElavonPaymentsNet.Models.Public;

/// <summary>Alipay payment details.</summary>
public sealed class AlipayPaymentMethod
{
    /// <summary>Language code used for the payment journey.</summary>
    public string? LanguageCode { get; init; }

    /// <summary>Merchant session key associated with the payment.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>Callback URL for post-authorization return.</summary>
    public string? CallbackUrl { get; init; }

    /// <summary>Shopper platform hint, for example mobile/web.</summary>
    public string? ShopperPlatform { get; init; }
}
