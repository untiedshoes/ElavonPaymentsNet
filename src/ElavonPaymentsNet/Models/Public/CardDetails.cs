namespace ElavonPaymentsNet.Models.Public;

/// <summary>Card details for a payment or token creation.</summary>
public sealed class CardDetails
{
    /// <summary>
    /// The full card number (PAN). Required for raw card flows.
    /// Not required when using <see cref="CardIdentifier"/> + <see cref="MerchantSessionKey"/>.
    /// </summary>
    public string? CardNumber { get; init; }

    /// <summary>
    /// Expiry date in MMYY format. Required for raw card flows.
    /// Not required when using <see cref="CardIdentifier"/> + <see cref="MerchantSessionKey"/>.
    /// </summary>
    public string? ExpiryDate { get; init; }

    /// <summary>The card security code (CVV/CVC).</summary>
    public string? SecurityCode { get; init; }

    /// <summary>The cardholder's name as it appears on the card.</summary>
    public string? CardholderName { get; init; }

    /// <summary>
    /// Merchant session key used with a card identifier in drop-in/HPP style flows.
    /// </summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>
    /// Card identifier created via the card-identifiers API for drop-in/HPP style flows.
    /// </summary>
    public string? CardIdentifier { get; init; }
}
