namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after completing a 3D Secure challenge.</summary>
public sealed class Complete3DsResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>The final transaction status after 3DS completion.</summary>
    public string? Status { get; init; }
}
