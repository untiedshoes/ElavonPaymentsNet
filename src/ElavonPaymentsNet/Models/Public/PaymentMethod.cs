namespace ElavonPaymentsNet.Models.Public;

/// <summary>Payment method — either a card or a stored token.</summary>
public sealed class PaymentMethod
{
    /// <summary>Card details. Provide either <see cref="Card"/> or <see cref="Token"/>, not both.</summary>
    public CardDetails? Card { get; init; }

    /// <summary>Stored card token. Provide either <see cref="Card"/> or <see cref="Token"/>, not both.</summary>
    public string? Token { get; init; }
}
