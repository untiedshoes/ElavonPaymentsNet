namespace ElavonPaymentsNet.Models.Public;

/// <summary>PayPal payment details.</summary>
public sealed class PayPalPaymentMethod
{
    /// <summary>Merchant session key for the PayPal wallet flow.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>Callback URL for post-authorization return.</summary>
    public string? CallbackUrl { get; init; }
}
