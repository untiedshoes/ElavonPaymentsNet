using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public;

/// <summary>Payment method — either a card or a stored token.</summary>
public sealed class PaymentMethod
{
    /// <summary>Card details. Provide either <see cref="Card"/> or <see cref="Token"/>, not both.</summary>
    public CardDetails? Card { get; init; }

    /// <summary>Stored card token. Provide either <see cref="Card"/> or <see cref="Token"/>, not both.</summary>
    public string? Token { get; init; }

    /// <summary>PayPal payment details.</summary>
    [JsonPropertyName("paypal")]
    public PayPalPaymentMethod? PayPal { get; init; }

    /// <summary>Apple Pay payment details.</summary>
    public ApplePayPaymentMethod? ApplePay { get; init; }

    /// <summary>Google Pay payment details.</summary>
    public GooglePayPaymentMethod? GooglePay { get; init; }

    /// <summary>iDEAL payment details.</summary>
    public IdealPaymentMethod? Ideal { get; init; }

    /// <summary>Alipay payment details.</summary>
    public AlipayPaymentMethod? Alipay { get; init; }

    /// <summary>WeChat Pay payment details.</summary>
    [JsonPropertyName("wechatpay")]
    public WechatPayPaymentMethod? WechatPay { get; init; }

    /// <summary>EPS payment details.</summary>
    public EpsPaymentMethod? Eps { get; init; }

    /// <summary>Trustly payment details.</summary>
    public TrustlyPaymentMethod? Trustly { get; init; }
}
