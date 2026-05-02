using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after completing a 3D Secure v2 challenge.</summary>
public sealed class Complete3DsResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>Access Control Server (ACS) transaction ID assigned by the card issuer.</summary>
    public string? AcsTransId { get; init; }

    /// <summary>Directory Server (DS) transaction ID assigned by the card scheme.</summary>
    public string? DsTransId { get; init; }

    /// <summary>The type of the transaction, e.g. "Payment", "Deferred".</summary>
    public string? TransactionType { get; init; }

    /// <summary>The final transaction status, e.g. "Ok", "NotAuthed", "Rejected".</summary>
    public string? Status { get; init; }

    /// <summary>A numeric status code accompanying the status.</summary>
    public string? StatusCode { get; init; }

    /// <summary>A human-readable description of the status.</summary>
    public string? StatusDetail { get; init; }

    /// <summary>Bank-specific decline code. "65" or "1A" indicates a soft decline.</summary>
    public string? BankResponseCode { get; init; }

    /// <summary>The authorisation code returned by the merchant bank.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? BankAuthorisationCode { get; init; }

    /// <summary>Opayo's unique authorisation reference for successfully authorised transactions.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? RetrievalReference { get; init; }

    /// <summary>3D Secure authentication result.</summary>
    [JsonPropertyName("3DSecure")]
    public ThreeDSecureInfo? ThreeDSecure { get; init; }

    /// <summary>The currency of the transaction in ISO 4217 format.</summary>
    public string? Currency { get; init; }
}
