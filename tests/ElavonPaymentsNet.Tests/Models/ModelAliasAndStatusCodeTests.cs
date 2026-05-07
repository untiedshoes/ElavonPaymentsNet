using System.Text.Json;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Tests.Models;

[Trait("Category", "Unit")]
public class ModelAliasAndStatusCodeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact(DisplayName = "MerchantRiskIndicator ConflictingRawAndTyped Throws")]
    public void MerchantRiskIndicator_ConflictingRawAndTyped_Throws()
    {
        Assert.Throws<ArgumentException>(() => new MerchantRiskIndicator
        {
            DeliveryTimeframe = "GatewaySpecificCode",
            DeliveryTimeframeIndicator = DeliveryTimeframeIndicator.OvernightShipping
        });
    }

    [Fact(DisplayName = "MerchantRiskIndicator MatchingRawAndTyped Allowed")]
    public void MerchantRiskIndicator_MatchingRawAndTyped_Allowed()
    {
        var model = new MerchantRiskIndicator
        {
            DeliveryTimeframeIndicator = DeliveryTimeframeIndicator.OvernightShipping,
            DeliveryTimeframe = "OvernightShipping"
        };

        Assert.Equal("OvernightShipping", model.DeliveryTimeframe);
        Assert.Equal(DeliveryTimeframeIndicator.OvernightShipping, model.DeliveryTimeframeIndicator);
    }

    [Fact(DisplayName = "StrongCustomerAuthentication ConflictingAliases Throw")]
    public void StrongCustomerAuthentication_ConflictingAliases_Throw()
    {
        Assert.Throws<ArgumentException>(() => new StrongCustomerAuthentication
        {
            ThreeDSExemptionIndicator = "GatewaySpecificCode",
            ThreeDSExemptionIndicatorType = ThreeDSExemptionIndicatorType.TransactionRiskAnalysis
        });
    }

    [Fact(DisplayName = "StrongCustomerAuthentication MatchingAliases Allowed")]
    public void StrongCustomerAuthentication_MatchingAliases_Allowed()
    {
        var model = new StrongCustomerAuthentication
        {
            ThreeDSExemptionIndicatorType = ThreeDSExemptionIndicatorType.TransactionRiskAnalysis,
            ThreeDSRequestorExemptionIndicator = "TransactionRiskAnalysis"
        };

        Assert.Equal("TransactionRiskAnalysis", model.ThreeDSExemptionIndicator);
        Assert.Equal("TransactionRiskAnalysis", model.ThreeDSRequestorExemptionIndicator);
        Assert.Equal(ThreeDSExemptionIndicatorType.TransactionRiskAnalysis, model.ThreeDSExemptionIndicatorType);
    }

    [Fact(DisplayName = "Complete3DsResponse StatusCode ReadsStringOrNumber")]
    public void Complete3DsResponse_StatusCode_ReadsStringOrNumber()
    {
        var fromString = JsonSerializer.Deserialize<Complete3DsResponse>("{\"statusCode\":\"200\"}", JsonOptions);
        var fromNumber = JsonSerializer.Deserialize<Complete3DsResponse>("{\"statusCode\":200}", JsonOptions);

        Assert.Equal(200, fromString!.StatusCode);
        Assert.Equal(200, fromNumber!.StatusCode);
    }

    [Fact(DisplayName = "ApplePaySessionResponse StatusCode PreservesRawString")]
    public void ApplePaySessionResponse_StatusCode_PreservesRawString()
    {
        var response = JsonSerializer.Deserialize<ApplePaySessionResponse>("{\"statusCode\":\"A001\"}", JsonOptions);

        Assert.Equal("A001", response!.StatusCode);
    }
}