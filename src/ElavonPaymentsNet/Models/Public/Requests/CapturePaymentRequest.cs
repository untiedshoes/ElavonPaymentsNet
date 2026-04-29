namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for capturing a previously deferred or authorised payment.</summary>
public sealed class CapturePaymentRequest
{
    /// <summary>Amount to capture in the smallest currency unit. Must not exceed the authorised amount.</summary>
    public required int Amount { get; init; }
}
