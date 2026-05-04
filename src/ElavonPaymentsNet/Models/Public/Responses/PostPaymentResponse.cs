using ElavonPaymentsNet.Models.Public;
using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after a capture or refund operation.</summary>
public sealed class PostPaymentResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>The operation status, e.g. "Ok".</summary>
    public string? Status { get; init; }

    /// <summary>
    /// Strongly typed view of <see cref="Status"/>.
    /// Unknown or newly introduced API statuses map to <see cref="TransactionStatusKind.Unknown"/>.
    /// </summary>
    [JsonIgnore]
    public TransactionStatusKind StatusKind => TransactionStatus.ParseKind(Status);
}
