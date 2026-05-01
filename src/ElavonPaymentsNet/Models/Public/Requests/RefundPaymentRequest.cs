namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for refunding a previously completed payment.</summary>
public sealed class RefundPaymentRequest
{
    /// <summary>Amount to refund in the smallest currency unit.</summary>
    public required int Amount { get; init; }

    /// <summary>A unique vendor-assigned transaction code for this refund.</summary>
    public required string VendorTxCode { get; init; }

    /// <summary>A short description of the refund transaction.</summary>
    public required string Description { get; init; }
}
