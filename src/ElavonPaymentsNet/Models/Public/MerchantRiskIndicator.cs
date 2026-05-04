using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Merchant-provided risk context for EMV 3DS.
/// These fields help issuers perform risk-based authentication decisions.
/// </summary>
public sealed class MerchantRiskIndicator
{
    private readonly string? _deliveryTimeframe;
    private readonly string? _preOrderPurchaseInd;
    private readonly string? _reorderItemsInd;
    private readonly string? _shipIndicator;

    /// <summary>
    /// Delivery email address for digital delivery, if applicable.
    /// </summary>
    public string? DeliveryEmailAddress { get; init; }

    /// <summary>
    /// Delivery timeframe indicator code as defined by your acquirer/gateway profile.
    /// </summary>
    public string? DeliveryTimeframe
    {
        get => _deliveryTimeframe;
        init => _deliveryTimeframe = value;
    }

    /// <summary>
    /// Typed alias for <see cref="DeliveryTimeframe"/>.
    /// </summary>
    [JsonIgnore]
    public DeliveryTimeframeIndicator? DeliveryTimeframeIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<DeliveryTimeframeIndicator>(_deliveryTimeframe);
        init => _deliveryTimeframe = ThreeDsIndicatorMapper.ToApi(value);
    }

    /// <summary>
    /// Pre-order purchase indicator code.
    /// </summary>
    public string? PreOrderPurchaseInd
    {
        get => _preOrderPurchaseInd;
        init => _preOrderPurchaseInd = value;
    }

    /// <summary>
    /// Typed alias for <see cref="PreOrderPurchaseInd"/>.
    /// </summary>
    [JsonIgnore]
    public PreOrderPurchaseIndicator? PreOrderPurchaseIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<PreOrderPurchaseIndicator>(_preOrderPurchaseInd);
        init => _preOrderPurchaseInd = ThreeDsIndicatorMapper.ToApi(value);
    }

    /// <summary>
    /// Expected fulfilment date for pre-orders in YYYYMMDD format.
    /// </summary>
    public string? PreOrderDate { get; init; }

    /// <summary>
    /// Reorder indicator code.
    /// </summary>
    public string? ReorderItemsInd
    {
        get => _reorderItemsInd;
        init => _reorderItemsInd = value;
    }

    /// <summary>
    /// Typed alias for <see cref="ReorderItemsInd"/>.
    /// </summary>
    [JsonIgnore]
    public ReorderItemsIndicator? ReorderItemsIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<ReorderItemsIndicator>(_reorderItemsInd);
        init => _reorderItemsInd = ThreeDsIndicatorMapper.ToApi(value);
    }

    /// <summary>
    /// Shipping indicator code (for example, billing-address match vs alternate address).
    /// </summary>
    public string? ShipIndicator
    {
        get => _shipIndicator;
        init => _shipIndicator = value;
    }

    /// <summary>
    /// Typed alias for <see cref="ShipIndicator"/>.
    /// </summary>
    [JsonIgnore]
    public ShipIndicatorType? ShipIndicatorType
    {
        get => ThreeDsIndicatorMapper.Parse<ShipIndicatorType>(_shipIndicator);
        init => _shipIndicator = ThreeDsIndicatorMapper.ToApi(value);
    }

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
