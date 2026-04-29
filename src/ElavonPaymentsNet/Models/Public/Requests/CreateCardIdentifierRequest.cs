namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>
/// Request model for creating a card identifier against a merchant session key.
/// This is typically called from the browser via Opayo's drop-in UI, but the
/// SDK exposes it for server-side flows that require it.
/// </summary>
public sealed class CreateCardIdentifierRequest
{
    /// <summary>The card details to tokenise into a card identifier.</summary>
    public required CardDetails CardDetails { get; init; }
}
