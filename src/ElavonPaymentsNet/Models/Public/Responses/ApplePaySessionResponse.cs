using System.Text.Json;

namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after requesting an Apple Pay merchant session.</summary>
public sealed class ApplePaySessionResponse
{
    /// <summary>
    /// The opaque Apple Pay session object to be passed to <c>completeMerchantValidation</c>
    /// in the browser. The exact structure is defined by Apple.
    /// </summary>
    public JsonElement? Session { get; init; }
}
