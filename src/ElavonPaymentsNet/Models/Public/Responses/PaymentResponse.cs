using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after creating or authorising a payment transaction.</summary>
public sealed class PaymentResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// The transaction status, e.g. "Ok", "NotAuthed", "Rejected", "3DAuth".
    /// </summary>
    public string? Status { get; init; }

    /// <summary>A numeric status code accompanying the status.</summary>
    public int? StatusCode { get; init; }

    /// <summary>A human-readable description of the status.</summary>
    public string? StatusDetail { get; init; }

    /// <summary>3D Secure result, if applicable.</summary>
    [JsonPropertyName("3DSecure")]
    public ThreeDSecureInfo? ThreeDSecure { get; init; }
}
