namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for processing a payment using a stored card token.</summary>
public sealed class PayWithTokenRequest
{
    /// <summary>A unique vendor-assigned transaction code.</summary>
    public required string VendorTxCode { get; init; }

    /// <summary>Amount in the smallest currency unit (e.g. pence for GBP).</summary>
    public required int Amount { get; init; }

    /// <summary>ISO 4217 currency code, e.g. "GBP".</summary>
    public required string Currency { get; init; }

    /// <summary>The stored card token to charge.</summary>
    public required string Token { get; init; }
}
