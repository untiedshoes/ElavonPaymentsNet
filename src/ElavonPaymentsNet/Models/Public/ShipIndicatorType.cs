namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Known indicator values for shipping destination profile.
/// </summary>
public enum ShipIndicatorType
{
    /// <summary>Shipping address matches the cardholder billing address.</summary>
    CardholderBillingAddress,

    /// <summary>Shipping address is another verified address for this cardholder.</summary>
    AnotherVerifiedAddress,

    /// <summary>Shipping address is different from billing and unverified.</summary>
    DifferentAddress,

    /// <summary>Store pickup.</summary>
    StorePickup,

    /// <summary>Digital goods shipment type.</summary>
    DigitalGoods,

    /// <summary>Travel and event tickets shipment type.</summary>
    TravelAndEventTickets,

    /// <summary>Other shipping profile.</summary>
    Other
}
