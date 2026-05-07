using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Merchant-provided risk context for EMV 3DS.
/// These fields help issuers perform risk-based authentication decisions.
/// For indicator pairs, set either the raw gateway code property or the typed alias.
/// Setting both with conflicting values throws <see cref="ArgumentException"/>.
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
        init => _deliveryTimeframe = SetIndicatorValue(_deliveryTimeframe, value, nameof(DeliveryTimeframe), nameof(DeliveryTimeframeIndicator));
    }

    /// <summary>
    /// Typed alias for <see cref="DeliveryTimeframe"/>.
    /// </summary>
    [JsonIgnore]
    public DeliveryTimeframeIndicator? DeliveryTimeframeIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<DeliveryTimeframeIndicator>(_deliveryTimeframe);
        init => _deliveryTimeframe = SetIndicatorValue(_deliveryTimeframe, ThreeDsIndicatorMapper.ToApi(value), nameof(DeliveryTimeframe), nameof(DeliveryTimeframeIndicator));
    }

    /// <summary>
    /// Pre-order purchase indicator code.
    /// </summary>
    public string? PreOrderPurchaseInd
    {
        get => _preOrderPurchaseInd;
        init => _preOrderPurchaseInd = SetIndicatorValue(_preOrderPurchaseInd, value, nameof(PreOrderPurchaseInd), nameof(PreOrderPurchaseIndicator));
    }

    /// <summary>
    /// Typed alias for <see cref="PreOrderPurchaseInd"/>.
    /// </summary>
    [JsonIgnore]
    public PreOrderPurchaseIndicator? PreOrderPurchaseIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<PreOrderPurchaseIndicator>(_preOrderPurchaseInd);
        init => _preOrderPurchaseInd = SetIndicatorValue(_preOrderPurchaseInd, ThreeDsIndicatorMapper.ToApi(value), nameof(PreOrderPurchaseInd), nameof(PreOrderPurchaseIndicator));
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
        init => _reorderItemsInd = SetIndicatorValue(_reorderItemsInd, value, nameof(ReorderItemsInd), nameof(ReorderItemsIndicator));
    }

    /// <summary>
    /// Typed alias for <see cref="ReorderItemsInd"/>.
    /// </summary>
    [JsonIgnore]
    public ReorderItemsIndicator? ReorderItemsIndicator
    {
        get => ThreeDsIndicatorMapper.Parse<ReorderItemsIndicator>(_reorderItemsInd);
        init => _reorderItemsInd = SetIndicatorValue(_reorderItemsInd, ThreeDsIndicatorMapper.ToApi(value), nameof(ReorderItemsInd), nameof(ReorderItemsIndicator));
    }

    /// <summary>
    /// Shipping indicator code (for example, billing-address match vs alternate address).
    /// </summary>
    public string? ShipIndicator
    {
        get => _shipIndicator;
        init => _shipIndicator = SetIndicatorValue(_shipIndicator, value, nameof(ShipIndicator), nameof(ShipIndicatorType));
    }

    /// <summary>
    /// Typed alias for <see cref="ShipIndicator"/>.
    /// </summary>
    [JsonIgnore]
    public ShipIndicatorType? ShipIndicatorType
    {
        get => ThreeDsIndicatorMapper.Parse<ShipIndicatorType>(_shipIndicator);
        init => _shipIndicator = SetIndicatorValue(_shipIndicator, ThreeDsIndicatorMapper.ToApi(value), nameof(ShipIndicator), nameof(ShipIndicatorType));
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

    private static string? SetIndicatorValue(string? currentValue, string? newValue, string rawProperty, string typedAliasProperty)
    {
        if (!string.IsNullOrWhiteSpace(currentValue)
            && !string.IsNullOrWhiteSpace(newValue)
            && !string.Equals(currentValue, newValue, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Conflicting values supplied. Set either {rawProperty} or {typedAliasProperty}, not both with different values.");
        }

        return newValue;
    }
}
