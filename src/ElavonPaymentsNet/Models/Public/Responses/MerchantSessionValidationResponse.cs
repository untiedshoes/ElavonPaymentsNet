namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after validating a merchant session key.</summary>
public sealed class MerchantSessionValidationResponse
{
    /// <summary>Whether the supplied merchant session key is currently valid.</summary>
    public bool Valid { get; init; }
}
