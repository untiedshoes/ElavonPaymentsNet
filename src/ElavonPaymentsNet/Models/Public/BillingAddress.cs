namespace ElavonPaymentsNet.Models.Public;

/// <summary>Billing address associated with a payment.</summary>
public sealed class BillingAddress
{
    /// <summary>First line of the street address.</summary>
    public required string Address1 { get; init; }
    /// <summary>Second line of the street address, if applicable.</summary>
    public string? Address2 { get; init; }
    /// <summary>Third line of the street address, if applicable.</summary>
    public string? Address3 { get; init; }
    /// <summary>City or town name.</summary>
    public required string City { get; init; }
    /// <summary>Postal or ZIP code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>ISO 3166-1 alpha-2 country code, e.g. "GB".</summary>
    public required string Country { get; init; }
}
