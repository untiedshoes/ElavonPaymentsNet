namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after tokenising a card.</summary>
public sealed class CreateTokenResponse
{
    /// <summary>The reusable card token.</summary>
    public string? Token { get; init; }
}
