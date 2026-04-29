namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>Request model for validating a merchant session key.</summary>
public sealed class MerchantSessionValidationRequest
{
    /// <summary>The merchant session key to validate.</summary>
    public required string MerchantSessionKey { get; init; }
}
