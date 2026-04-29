namespace ElavonPaymentsNet.Models.Public;

/// <summary>Card details for a payment or token creation.</summary>
public sealed class CardDetails
{
    /// <summary>The full card number (PAN).</summary>
    public required string CardNumber { get; init; }

    /// <summary>Expiry date in MMYY format.</summary>
    public required string ExpiryDate { get; init; }

    /// <summary>The card security code (CVV/CVC).</summary>
    public string? SecurityCode { get; init; }

    /// <summary>The cardholder's name as it appears on the card.</summary>
    public string? CardholderName { get; init; }
}
