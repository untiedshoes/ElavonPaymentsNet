namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>
/// Request model for linking a security code to an existing card identifier.
/// </summary>
public sealed class LinkCardIdentifierRequest
{
    /// <summary>The card security code (CVV/CV2) to link to the card identifier. 3–4 characters.</summary>
    public required string SecurityCode { get; init; }
}
