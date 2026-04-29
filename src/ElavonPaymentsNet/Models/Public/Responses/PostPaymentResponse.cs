namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after a capture or refund operation.</summary>
public sealed class PostPaymentResponse
{
    /// <summary>The Elavon-assigned transaction ID.</summary>
    public string? TransactionId { get; init; }

    /// <summary>The operation status, e.g. "Ok".</summary>
    public string? Status { get; init; }
}
