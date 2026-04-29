namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after creating a card identifier.</summary>
public sealed class CreateCardIdentifierResponse
{
    /// <summary>The card identifier token to use in subsequent payment requests.</summary>
    public string? CardIdentifier { get; init; }

    /// <summary>The expiry timestamp of the card identifier (ISO 8601).</summary>
    public string? Expiry { get; init; }

    /// <summary>The card scheme type, e.g. "Visa".</summary>
    public string? CardType { get; init; }
}
