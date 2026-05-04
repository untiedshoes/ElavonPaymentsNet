namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Known indicator values for merchant risk delivery timeframe.
/// </summary>
public enum DeliveryTimeframeIndicator
{
    /// <summary>Electronic delivery (no physical shipment).</summary>
    ElectronicDelivery,

    /// <summary>Same-day shipping.</summary>
    SameDayShipping,

    /// <summary>Overnight shipping.</summary>
    OvernightShipping,

    /// <summary>Shipment in two or more days.</summary>
    TwoOrMoreDaysShipping
}
