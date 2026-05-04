namespace ElavonPaymentsNet.Models.Public;

/// <summary>Google Pay payment details.</summary>
public sealed class GooglePayPaymentMethod
{
    /// <summary>Encoded Google Pay payload.</summary>
    public string? Payload { get; init; }

    /// <summary>Client IP address of the shopper.</summary>
    public string? ClientIpAddress { get; init; }

    /// <summary>Merchant session key associated with the wallet session.</summary>
    public string? MerchantSessionKey { get; init; }
}
