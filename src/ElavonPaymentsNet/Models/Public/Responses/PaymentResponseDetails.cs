using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Extended decline information returned by card schemes.</summary>
public sealed class AdditionalDeclineDetailInfo
{
    public string? AdditionalDeclineCode { get; init; }
    public string? AdditionalDeclineCodeDescription { get; init; }
    public string? AdditionalDeclineCodeCategory { get; init; }
}

/// <summary>AVS/CVC validation details returned by the gateway.</summary>
public sealed class AvsCvcCheckInfo
{
    public string? Status { get; init; }
    public string? Address { get; init; }
    public string? PostalCode { get; init; }
    public string? SecurityCode { get; init; }
}

/// <summary>Amount breakdown information returned by transaction responses.</summary>
public sealed class AmountInfo
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? TotalAmount { get; init; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? SaleAmount { get; init; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? SurchargeAmount { get; init; }
}

/// <summary>Payment method details returned in transaction responses.</summary>
public sealed class PaymentMethodResponse
{
    public CardPaymentMethodInfo? Card { get; init; }
    public ApplePayPaymentMethodInfo? Applepay { get; init; }
    public GooglePayPaymentMethodInfo? Googlepay { get; init; }
    public PayPalPaymentMethodInfo? Paypal { get; init; }
    public TrustlyPaymentMethodInfo? Trustly { get; init; }
    public IdealPaymentMethodInfo? Ideal { get; init; }
    public EpsPaymentMethodInfo? Eps { get; init; }
    public WechatPayPaymentMethodInfo? Wechatpay { get; init; }
    public AlipayPaymentMethodInfo? Alipay { get; init; }
}

/// <summary>Card-specific response metadata.</summary>
public sealed class CardPaymentMethodInfo
{
    public string? CardType { get; init; }
    public string? LastFourDigits { get; init; }
    public string? ExpiryDate { get; init; }
    public string? CardIdentifier { get; init; }
    public bool? Reusable { get; init; }
}

/// <summary>Apple Pay-specific response metadata.</summary>
public sealed class ApplePayPaymentMethodInfo
{
    public string? LastFourDigits { get; init; }
    public string? ExpiryDate { get; init; }
}

/// <summary>Google Pay-specific response metadata.</summary>
public sealed class GooglePayPaymentMethodInfo
{
    public string? LastFourDigits { get; init; }
    public string? ExpiryDate { get; init; }
}

/// <summary>PayPal-specific response metadata.</summary>
public sealed class PayPalPaymentMethodInfo
{
    public string? OrderId { get; init; }
    public string? PayerId { get; init; }
    public string? CaptureId { get; init; }
}

/// <summary>Trustly-specific response metadata.</summary>
public sealed class TrustlyPaymentMethodInfo
{
    public string? PaymentInfo { get; init; }
}

/// <summary>iDEAL-specific response metadata.</summary>
public sealed class IdealPaymentMethodInfo
{
    public string? PaymentInfo { get; init; }
}

/// <summary>EPS-specific response metadata.</summary>
public sealed class EpsPaymentMethodInfo
{
    public string? PaymentInfo { get; init; }
    public string? Bic { get; init; }
}

/// <summary>WeChat Pay-specific response metadata.</summary>
public sealed class WechatPayPaymentMethodInfo
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? UserAmount { get; init; }

    public string? UserCurrency { get; init; }
}

/// <summary>Alipay-specific response metadata.</summary>
public sealed class AlipayPaymentMethodInfo
{
}

/// <summary>Financial institution recipient details (MCC 6012 flows).</summary>
public sealed class FiRecipientInfo
{
    public string? AccountNumber { get; init; }
    public string? Surname { get; init; }
    public string? Postcode { get; init; }
    public string? DateOfBirth { get; init; }
}
