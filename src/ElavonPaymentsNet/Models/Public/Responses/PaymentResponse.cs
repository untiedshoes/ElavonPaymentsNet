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
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? StatusCode { get; init; }

    /// <summary>A human-readable description of the status.</summary>
    public string? StatusDetail { get; init; }

    /// <summary>3D Secure result, if applicable.</summary>
    [JsonPropertyName("3DSecure")]
    public ThreeDSecureInfo? ThreeDSecure { get; init; }

    /// <summary>
    /// URL of the card issuer's Access Control Server (ACS). Present when <see cref="Status"/> is
    /// "3DAuth" and the customer must complete a 3DS challenge.
    /// </summary>
    public string? AcsUrl { get; init; }

    /// <summary>
    /// The encoded challenge request to POST to the ACS. Present alongside <see cref="AcsUrl"/>.
    /// </summary>
    public string? CReq { get; init; }
}
