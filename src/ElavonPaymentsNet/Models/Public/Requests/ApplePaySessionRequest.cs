namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for obtaining an Apple Pay merchant session.</summary>
public sealed class ApplePaySessionRequest
{
    /// <summary>The Apple Pay validation URL provided in the browser's <c>onvalidatemerchant</c> event.</summary>
    public required string ValidationUrl { get; init; }

    /// <summary>Your registered Apple Pay merchant domain name.</summary>
    public required string DomainName { get; init; }
}
