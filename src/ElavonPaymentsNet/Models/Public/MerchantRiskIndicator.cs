namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Merchant-provided risk context for EMV 3DS.
/// These fields help issuers perform risk-based authentication decisions.
/// </summary>
public sealed class MerchantRiskIndicator
{
    /// <summary>
    /// Delivery email address for digital delivery, if applicable.
    /// </summary>
    public string? DeliveryEmailAddress { get; init; }

    /// <summary>
    /// Delivery timeframe indicator code as defined by your acquirer/gateway profile.
    /// </summary>
    public string? DeliveryTimeframe { get; init; }

    /// <summary>
    /// Pre-order purchase indicator code.
    /// </summary>
    public string? PreOrderPurchaseInd { get; init; }

    /// <summary>
    /// Expected fulfilment date for pre-orders in YYYYMMDD format.
    /// </summary>
    public string? PreOrderDate { get; init; }

    /// <summary>
    /// Reorder indicator code.
    /// </summary>
    public string? ReorderItemsInd { get; init; }

    /// <summary>
    /// Shipping indicator code (for example, billing-address match vs alternate address).
    /// </summary>
    public string? ShipIndicator { get; init; }

    /// <summary>
    /// Gift card amount in minor units when the order includes gift cards.
    /// </summary>
    public string? GiftCardAmount { get; init; }

    /// <summary>
    /// Gift card currency (ISO 4217), for example GBP.
    /// </summary>
    public string? GiftCardCurrency { get; init; }

    /// <summary>
    /// Number of gift cards purchased in this order.
    /// </summary>
    public string? GiftCardCount { get; init; }
}
