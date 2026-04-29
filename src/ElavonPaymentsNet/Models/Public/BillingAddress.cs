namespace ElavonPaymentsNet.Models.Public;

/// <summary>Billing address associated with a payment.</summary>
public sealed class BillingAddress
{
    public required string Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? Address3 { get; init; }
    public required string City { get; init; }
    public string? PostalCode { get; init; }

    /// <summary>ISO 3166-1 alpha-2 country code, e.g. "GB".</summary>
    public required string Country { get; init; }
}
