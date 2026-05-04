using ElavonPaymentsNet.Models.Public;
using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after creating or authorising a payment transaction.</summary>
public sealed class PaymentResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>The type of the transaction, e.g. "Payment", "Deferred", "Repeat".</summary>
    public string? TransactionType { get; init; }

    /// <summary>Access Control Server (ACS) transaction ID assigned by the card issuer, if applicable.</summary>
    public string? AcsTransId { get; init; }

    /// <summary>Directory Server (DS) transaction ID assigned by card scheme, if applicable.</summary>
    public string? DsTransId { get; init; }

    /// <summary>
    /// The transaction status, e.g. "Ok", "NotAuthed", "Rejected", "3DAuth".
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Strongly typed view of <see cref="Status"/>.
    /// Unknown or newly introduced API statuses map to <see cref="TransactionStatusKind.Unknown"/>.
    /// </summary>
    [JsonIgnore]
    public TransactionStatusKind StatusKind => TransactionStatus.ParseKind(Status);

    /// <summary>A numeric status code accompanying the status.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? StatusCode { get; init; }

    /// <summary>A human-readable description of the status.</summary>
    public string? StatusDetail { get; init; }

    /// <summary>Additional decline details returned by card schemes, if present.</summary>
    public AdditionalDeclineDetailInfo? AdditionalDeclineDetail { get; init; }

    /// <summary>Opayo retrieval reference for successful authorisations, if present.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? RetrievalReference { get; init; }

    /// <summary>Optional settlement reference text echoed by the gateway.</summary>
    public string? SettlementReferenceText { get; init; }

    /// <summary>Bank response code (decline/success code from acquirer), if returned.</summary>
    public string? BankResponseCode { get; init; }

    /// <summary>Authorisation code returned by the merchant bank, if present.</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? BankAuthorisationCode { get; init; }

    /// <summary>AVS/CVC check results for the transaction, if present.</summary>
    public AvsCvcCheckInfo? AvsCvcCheck { get; init; }

    /// <summary>Payment method details returned for the selected tender type.</summary>
    public PaymentMethodResponse? PaymentMethod { get; init; }

    /// <summary>Amount details for the transaction, if present.</summary>
    public AmountInfo? Amount { get; init; }

    /// <summary>The transaction currency in ISO 4217 format, if present.</summary>
    public string? Currency { get; init; }

    /// <summary>3D Secure result, if applicable.</summary>
    [JsonPropertyName("3DSecure")]
    public ThreeDSecureInfo? ThreeDSecure { get; init; }

    /// <summary>Financial institution recipient data for MCC 6012 merchants, if provided.</summary>
    public FiRecipientInfo? FiRecipient { get; init; }

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
