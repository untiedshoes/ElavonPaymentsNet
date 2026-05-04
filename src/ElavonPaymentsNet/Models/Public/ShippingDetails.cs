namespace ElavonPaymentsNet.Models.Public;

/// <summary>Shipping recipient and address details.</summary>
public sealed class ShippingDetails
{
    /// <summary>Recipient first name.</summary>
    public string? RecipientFirstName { get; init; }

    /// <summary>Recipient last name.</summary>
    public string? RecipientLastName { get; init; }

    /// <summary>First line of the shipping address.</summary>
    public string? ShippingAddress1 { get; init; }

    /// <summary>Second line of the shipping address.</summary>
    public string? ShippingAddress2 { get; init; }

    /// <summary>Third line of the shipping address.</summary>
    public string? ShippingAddress3 { get; init; }

    /// <summary>Shipping city or town.</summary>
    public string? ShippingCity { get; init; }

    /// <summary>Shipping postal or ZIP code.</summary>
    public string? ShippingPostalCode { get; init; }

    /// <summary>Shipping country code (ISO 3166-1 alpha-2).</summary>
    public string? ShippingCountry { get; init; }

    /// <summary>Shipping state or county code.</summary>
    public string? ShippingState { get; init; }
}
