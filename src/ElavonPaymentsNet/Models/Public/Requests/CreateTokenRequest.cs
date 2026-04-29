namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for tokenising a card for future use.</summary>
public sealed class CreateTokenRequest
{
    /// <summary>Card details to tokenise.</summary>
    public required CardDetails Card { get; init; }
}
