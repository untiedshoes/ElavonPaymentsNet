namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for obtaining an Apple Pay merchant session.</summary>
public sealed class ApplePaySessionRequest
{
    /// <summary>The Opayo vendor name linked to your API authentication credentials.</summary>
    public required string VendorName { get; init; }

    /// <summary>The domain registered with Opayo from which the Apple Pay transaction will originate.</summary>
    public required string Domain { get; init; }
}
