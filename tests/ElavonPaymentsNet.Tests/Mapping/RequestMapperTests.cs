using ElavonPaymentsNet.Mapping;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

namespace ElavonPaymentsNet.Tests.Mapping;

[Trait("Category", "Unit")]
public class RequestMapperTests
{
    /// <summary>
    /// Verifies that a payment transaction request maps all core fields and card details.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Payment MapsTransactionType")]
    public void CreateTransactionRequest_Payment_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-001",
            Amount = 1000,
            Currency = "GBP",
            Description = "Test payment",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails
                {
                    CardNumber = "4929000000006",
                    ExpiryDate = "1225",
                    SecurityCode = "123",
                    CardholderName = "Test User"
                }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Payment", dto.TransactionType);
        Assert.Equal("TX-001", dto.VendorTxCode);
        Assert.Equal(1000, dto.Amount);
        Assert.Equal("GBP", dto.Currency);
        Assert.Equal("Test payment", dto.Description);
        Assert.NotNull(dto.PaymentMethod.Card);
        Assert.Equal("4929000000006", dto.PaymentMethod.Card!.CardNumber);
        Assert.Equal("1225", dto.PaymentMethod.Card.ExpiryDate);
        Assert.Equal("123", dto.PaymentMethod.Card.SecurityCode);
        Assert.Equal("Test User", dto.PaymentMethod.Card.CardholderName);
    }

    /// <summary>
    /// Verifies that an authorise transaction maps to the expected transaction type.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Authorise MapsTransactionType")]
    public void CreateTransactionRequest_Authorise_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Authorise,
            VendorTxCode = "TX-002",
            Amount = 500,
            Currency = "GBP",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1225", SecurityCode = "123" }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Authorise", dto.TransactionType);
        Assert.Equal("TX-002", dto.VendorTxCode);
    }

    /// <summary>
    /// Verifies that a deferred transaction maps to the expected transaction type.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Deferred MapsTransactionType")]
    public void CreateTransactionRequest_Deferred_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Deferred,
            VendorTxCode = "TX-003",
            Amount = 750,
            Currency = "GBP",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1225" }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Deferred", dto.TransactionType);
    }

    /// <summary>
    /// Verifies that an authenticate transaction maps to the expected transaction type.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Authenticate MapsTransactionType")]
    public void CreateTransactionRequest_Authenticate_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Authenticate,
            VendorTxCode = "TX-003A",
            Amount = 0,
            Currency = "GBP",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1225", SecurityCode = "123" }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Authenticate", dto.TransactionType);
    }

    /// <summary>
    /// Verifies that a repeat transaction maps the related transaction identifier.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Repeat MapsTransactionType")]
    public void CreateTransactionRequest_Repeat_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Repeat,
            VendorTxCode = "TX-004",
            Amount = 1000,
            Currency = "GBP",
            RelatedTransactionId = "original-tx-id"
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Repeat", dto.TransactionType);
        Assert.Equal("original-tx-id", dto.ReferenceTransactionId);
    }

    /// <summary>
    /// Verifies that a refund transaction maps to the expected transaction type and reference.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest Refund MapsTransactionType")]
    public void CreateTransactionRequest_Refund_MapsTransactionType()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Refund,
            VendorTxCode = "TX-004R",
            Amount = 250,
            Currency = "GBP",
            RelatedTransactionId = "original-tx-id"
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Refund", dto.TransactionType);
        Assert.Equal("original-tx-id", dto.ReferenceTransactionId);
    }

    /// <summary>
    /// Verifies that billing address fields are mapped correctly to the DTO.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithBillingAddress MapsAllAddressFields")]
    public void CreateTransactionRequest_WithBillingAddress_MapsAllAddressFields()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-005",
            Amount = 1000,
            Currency = "GBP",
            Description = "Test",
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails { CardNumber = "4929000000006", ExpiryDate = "1225" }
            },
            BillingAddress = new BillingAddress
            {
                Address1 = "88 Test Street",
                City = "London",
                PostalCode = "EC1A 1BB",
                Country = "GB"
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.NotNull(dto.BillingAddress);
        Assert.Equal("88 Test Street", dto.BillingAddress!.Address1);
        Assert.Equal("London", dto.BillingAddress.City);
        Assert.Equal("EC1A 1BB", dto.BillingAddress.PostalCode);
        Assert.Equal("GB", dto.BillingAddress.Country);
    }

    /// <summary>
    /// Verifies that pay-with-token requests map the token into payment method and omit card details.
    /// </summary>
    [Fact(DisplayName = "PayWithTokenRequest MapsTokenIntoPaymentMethod")]
    public void PayWithTokenRequest_MapsTokenIntoPaymentMethod()
    {
        var request = new PayWithTokenRequest
        {
            VendorTxCode = "TX-006",
            Amount = 1200,
            Currency = "GBP",
            Token = "stored-card-token"
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Payment", dto.TransactionType);
        Assert.Equal("stored-card-token", dto.PaymentMethod.Token);
        Assert.Null(dto.PaymentMethod.Card);
    }

    /// <summary>
    /// Verifies that customer name fields are mapped to the DTO.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithCustomerName MapsBothNameFields")]
    public void CreateTransactionRequest_WithCustomerName_MapsBothNameFields()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-007",
            Amount = 100,
            Currency = "GBP",
            CustomerFirstName = "Craig",
            CustomerLastName = "Richards"
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("Craig", dto.CustomerFirstName);
        Assert.Equal("Richards", dto.CustomerLastName);
    }

    /// <summary>
    /// Verifies that Apply3DSecure maps to its string representation when set.
    /// </summary>
    [Theory]
    [InlineData(Apply3DSecureOption.Disable, "Disable")]
    [InlineData(Apply3DSecureOption.Force, "Force")]
    [InlineData(Apply3DSecureOption.ForceIgnoringRules, "ForceIgnoringRules")]
    [InlineData(Apply3DSecureOption.UseMSPSetting, "UseMSPSetting")]
    public void CreateTransactionRequest_WithApply3DSecure_MapsToString(Apply3DSecureOption option, string expected)
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-008",
            Amount = 100,
            Currency = "GBP",
            Apply3DSecure = option
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal(expected, dto.Apply3DSecure);
    }

    /// <summary>
    /// Verifies that Apply3DSecure maps to null when not set, so it is omitted from the wire payload.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithoutApply3DSecure MapsToNull")]
    public void CreateTransactionRequest_WithoutApply3DSecure_MapsToNull()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-009",
            Amount = 100,
            Currency = "GBP"
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Null(dto.Apply3DSecure);
    }

    /// <summary>
    /// Verifies that strongCustomerAuthentication is preserved on payment transactions.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithStrongCustomerAuthentication MapsObject")]
    public void CreateTransactionRequest_WithStrongCustomerAuthentication_MapsObject()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-010",
            Amount = 100,
            Currency = "GBP",
            StrongCustomerAuthentication = new StrongCustomerAuthentication
            {
                BrowserIP = "203.0.113.10",
                BrowserLanguage = "en-GB",
                BrowserUserAgent = "Mozilla/5.0",
                ThreeDSRequestorChallengeInd = "03"
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.NotNull(dto.StrongCustomerAuthentication);
        Assert.Equal("203.0.113.10", dto.StrongCustomerAuthentication!.BrowserIP);
        Assert.Equal("en-GB", dto.StrongCustomerAuthentication.BrowserLanguage);
        Assert.Equal("Mozilla/5.0", dto.StrongCustomerAuthentication.BrowserUserAgent);
        Assert.Equal("03", dto.StrongCustomerAuthentication.ThreeDSRequestorChallengeInd);
    }

    /// <summary>
    /// Verifies that advanced 3DS metadata (risk, prior auth, exemption) is preserved.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithAdvanced3DSMetadata MapsObject")]
    public void CreateTransactionRequest_WithAdvanced3DSMetadata_MapsObject()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "TX-011",
            Amount = 100,
            Currency = "GBP",
            StrongCustomerAuthentication = new StrongCustomerAuthentication
            {
                BrowserIP = "203.0.113.10",
                ThreeDSRequestorExemptionIndicator = "lowValue",
                MerchantRiskIndicator = new MerchantRiskIndicator
                {
                    DeliveryEmailAddress = "shopper@example.com",
                    DeliveryTimeframe = "01",
                    ReorderItemsInd = "01"
                },
                ThreeDSRequestorPriorAuthenticationInfo = new ThreeDSRequestorPriorAuthenticationInfo
                {
                    ThreeDSReqPriorAuthMethod = "02",
                    ThreeDSReqPriorAuthTimestamp = "202605041030",
                    ThreeDSReqPriorRef = "AUTH-REF-123"
                }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.NotNull(dto.StrongCustomerAuthentication);
        Assert.Equal("lowValue", dto.StrongCustomerAuthentication!.ThreeDSRequestorExemptionIndicator);

        Assert.NotNull(dto.StrongCustomerAuthentication.MerchantRiskIndicator);
        Assert.Equal("shopper@example.com", dto.StrongCustomerAuthentication.MerchantRiskIndicator!.DeliveryEmailAddress);
        Assert.Equal("01", dto.StrongCustomerAuthentication.MerchantRiskIndicator.DeliveryTimeframe);
        Assert.Equal("01", dto.StrongCustomerAuthentication.MerchantRiskIndicator.ReorderItemsInd);

        Assert.NotNull(dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo);
        Assert.Equal("02", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo!.ThreeDSReqPriorAuthMethod);
        Assert.Equal("202605041030", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo.ThreeDSReqPriorAuthTimestamp);
        Assert.Equal("AUTH-REF-123", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo.ThreeDSReqPriorRef);
    }
}
