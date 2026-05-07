using ElavonPaymentsNet.Models.Public;
using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after post-payment operations (capture, refund, void).</summary>
public sealed class PostPaymentResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// The operation status.
    /// Refund typically reflects gateway transaction status (for example "Ok").
    /// Capture/void currently return "InstructionAccepted" when the instruction endpoint succeeds.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Human-readable detail accompanying the status.</summary>
    public string? StatusDetail { get; init; }

    /// <summary>
    /// Strongly typed view of <see cref="Status"/>.
    /// Unknown or newly introduced API statuses map to <see cref="TransactionStatusKind.Unknown"/>.
    /// </summary>
    [JsonIgnore]
    public TransactionStatusKind StatusKind => TransactionStatus.ParseKind(Status);
}
