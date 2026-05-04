namespace ElavonPaymentsNet.Models.Public;

/// <summary>Apple Pay payment details.</summary>
public sealed class ApplePayPaymentMethod
{
    /// <summary>Base64-encoded Apple Pay payment data payload.</summary>
    public string? PaymentData { get; init; }

    /// <summary>Client IP address of the shopper.</summary>
    public string? ClientIpAddress { get; init; }

    /// <summary>Merchant session key associated with the wallet session.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>Session validation token from Apple validation flow.</summary>
    public string? SessionValidationToken { get; init; }

    /// <summary>Application data supplied by Apple Pay, if available.</summary>
    public string? ApplicationData { get; init; }

    /// <summary>Display name of the shopper payment method.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Payment method type (for example debit/credit).</summary>
    public string? PaymentMethodType { get; init; }
}
