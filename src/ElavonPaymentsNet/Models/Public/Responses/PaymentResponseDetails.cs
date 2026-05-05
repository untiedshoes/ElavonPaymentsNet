using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Extended decline information returned by card schemes.</summary>
public sealed class AdditionalDeclineDetailInfo
{
    /// <summary>Scheme/acquirer decline code, if supplied.</summary>
    public string? AdditionalDeclineCode { get; init; }

    /// <summary>Human-readable description for <see cref="AdditionalDeclineCode"/>.</summary>
    public string? AdditionalDeclineCodeDescription { get; init; }

    /// <summary>Category/grouping for the additional decline code.</summary>
    public string? AdditionalDeclineCodeCategory { get; init; }
}

/// <summary>AVS/CVC validation details returned by the gateway.</summary>
public sealed class AvsCvcCheckInfo
{
    /// <summary>Overall AVS/CVC check outcome.</summary>
    public string? Status { get; init; }

    /// <summary>Address line match result.</summary>
    public string? Address { get; init; }

    /// <summary>Postal code match result.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Card security code (CVV/CVC) check result.</summary>
    public string? SecurityCode { get; init; }
}

/// <summary>Amount breakdown information returned by transaction responses.</summary>
public sealed class AmountInfo
{
    /// <summary>Total amount in minor units.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? TotalAmount { get; init; }

    /// <summary>Sale/authorised amount in minor units.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? SaleAmount { get; init; }

    /// <summary>Surcharge amount in minor units, when present.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? SurchargeAmount { get; init; }
}

/// <summary>Payment method details returned in transaction responses.</summary>
public sealed class PaymentMethodResponse
{
    /// <summary>Card payment metadata.</summary>
    public CardPaymentMethodInfo? Card { get; init; }

    /// <summary>Apple Pay payment metadata.</summary>
    public ApplePayPaymentMethodInfo? Applepay { get; init; }

    /// <summary>Google Pay payment metadata.</summary>
    public GooglePayPaymentMethodInfo? Googlepay { get; init; }

    /// <summary>PayPal payment metadata.</summary>
    public PayPalPaymentMethodInfo? Paypal { get; init; }

    /// <summary>Trustly payment metadata.</summary>
    public TrustlyPaymentMethodInfo? Trustly { get; init; }

    /// <summary>iDEAL payment metadata.</summary>
    public IdealPaymentMethodInfo? Ideal { get; init; }

    /// <summary>EPS payment metadata.</summary>
    public EpsPaymentMethodInfo? Eps { get; init; }

    /// <summary>WeChat Pay payment metadata.</summary>
    public WechatPayPaymentMethodInfo? Wechatpay { get; init; }

    /// <summary>Alipay payment metadata.</summary>
    public AlipayPaymentMethodInfo? Alipay { get; init; }
}

/// <summary>Card-specific response metadata.</summary>
public sealed class CardPaymentMethodInfo
{
    /// <summary>Card brand/type, for example Visa or MasterCard.</summary>
    public string? CardType { get; init; }

    /// <summary>Last four digits of the PAN.</summary>
    public string? LastFourDigits { get; init; }

    /// <summary>Card expiry in MMYY format.</summary>
    public string? ExpiryDate { get; init; }

    /// <summary>Gateway-issued card identifier.</summary>
    public string? CardIdentifier { get; init; }

    /// <summary>Indicates whether the card identifier is reusable.</summary>
    public bool? Reusable { get; init; }
}

/// <summary>Apple Pay-specific response metadata.</summary>
public sealed class ApplePayPaymentMethodInfo
{
    /// <summary>Last four digits of the underlying PAN.</summary>
    public string? LastFourDigits { get; init; }

    /// <summary>Card expiry in MMYY format.</summary>
    public string? ExpiryDate { get; init; }
}

/// <summary>Google Pay-specific response metadata.</summary>
public sealed class GooglePayPaymentMethodInfo
{
    /// <summary>Last four digits of the underlying PAN.</summary>
    public string? LastFourDigits { get; init; }

    /// <summary>Card expiry in MMYY format.</summary>
    public string? ExpiryDate { get; init; }
}

/// <summary>PayPal-specific response metadata.</summary>
public sealed class PayPalPaymentMethodInfo
{
    /// <summary>PayPal order identifier.</summary>
    public string? OrderId { get; init; }

    /// <summary>PayPal payer identifier.</summary>
    public string? PayerId { get; init; }

    /// <summary>PayPal capture identifier.</summary>
    public string? CaptureId { get; init; }
}

/// <summary>Trustly-specific response metadata.</summary>
public sealed class TrustlyPaymentMethodInfo
{
    /// <summary>Provider-specific payment reference info.</summary>
    public string? PaymentInfo { get; init; }
}

/// <summary>iDEAL-specific response metadata.</summary>
public sealed class IdealPaymentMethodInfo
{
    /// <summary>Provider-specific payment reference info.</summary>
    public string? PaymentInfo { get; init; }
}

/// <summary>EPS-specific response metadata.</summary>
public sealed class EpsPaymentMethodInfo
{
    /// <summary>Provider-specific payment reference info.</summary>
    public string? PaymentInfo { get; init; }

    /// <summary>Bank identifier code where returned.</summary>
    public string? Bic { get; init; }
}

/// <summary>WeChat Pay-specific response metadata.</summary>
public sealed class WechatPayPaymentMethodInfo
{
    /// <summary>Amount in the shopper currency (minor units), if provided.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? UserAmount { get; init; }

    /// <summary>Shopper currency code for <see cref="UserAmount"/>.</summary>
    public string? UserCurrency { get; init; }
}

/// <summary>Alipay-specific response metadata.</summary>
public sealed class AlipayPaymentMethodInfo
{
}

/// <summary>Financial institution recipient details (MCC 6012 flows).</summary>
public sealed class FiRecipientInfo
{
    /// <summary>Recipient account number for eligible financial transactions.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Recipient surname.</summary>
    public string? Surname { get; init; }

    /// <summary>Recipient postal/ZIP code.</summary>
    public string? Postcode { get; init; }

    /// <summary>Recipient date of birth (format depends on acquirer requirements).</summary>
    public string? DateOfBirth { get; init; }
}
