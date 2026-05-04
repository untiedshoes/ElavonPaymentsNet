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
                AcctID = "acct-123",
                Website = "https://mydomain.com",
                ThreeDSExemptionIndicatorType = ThreeDSExemptionIndicatorType.TransactionRiskAnalysis,
                ThreeDSRequestorAuthenticationInfo = new ThreeDSRequestorAuthenticationInfo
                {
                    ThreeDSReqAuthData = "string",
                    ThreeDSReqAuthMethod = "LoginWithThreeDSRequestorCredentials",
                    ThreeDSReqAuthTimestamp = "201810011445"
                },
                AcctInfo = new AccountInfo
                {
                    ChAccAgeInd = "MoreThanSixtyDays",
                    ChAccDate = "20180925",
                    SuspiciousAccActivity = "NotSuspicious"
                },
                MerchantRiskIndicator = new MerchantRiskIndicator
                {
                    DeliveryEmailAddress = "shopper@example.com",
                    DeliveryTimeframeIndicator = DeliveryTimeframeIndicator.OvernightShipping,
                    GiftCardAmount = "123",
                    GiftCardCount = "2",
                    PreOrderDate = "20200220",
                    PreOrderPurchaseIndicator = PreOrderPurchaseIndicator.MerchandiseAvailable,
                    ReorderItemsIndicator = ReorderItemsIndicator.Reordered,
                    ShipIndicatorType = ShipIndicatorType.CardholderBillingAddress
                },
                ThreeDSRequestorPriorAuthenticationInfo = new ThreeDSRequestorPriorAuthenticationInfo
                {
                    ThreeDSReqPriorAuthData = "data",
                    ThreeDSReqPriorAuthMethod = "FrictionlessAuthentication",
                    ThreeDSReqPriorAuthTimestamp = "201901011645",
                    ThreeDSReqPriorRef = "2cd842f5-da5d-40b7-8ae6-6ce61cc7b580"
                }
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.NotNull(dto.StrongCustomerAuthentication);
        Assert.Equal("acct-123", dto.StrongCustomerAuthentication!.AcctID);
        Assert.Equal("https://mydomain.com", dto.StrongCustomerAuthentication.Website);
        Assert.Equal("TransactionRiskAnalysis", dto.StrongCustomerAuthentication.ThreeDSExemptionIndicator);
        Assert.Equal(ThreeDSExemptionIndicatorType.TransactionRiskAnalysis, dto.StrongCustomerAuthentication.ThreeDSExemptionIndicatorType);
        Assert.Equal("TransactionRiskAnalysis", dto.StrongCustomerAuthentication.ThreeDSRequestorExemptionIndicator);

        Assert.NotNull(dto.StrongCustomerAuthentication.ThreeDSRequestorAuthenticationInfo);
        Assert.Equal("string", dto.StrongCustomerAuthentication.ThreeDSRequestorAuthenticationInfo!.ThreeDSReqAuthData);
        Assert.Equal("LoginWithThreeDSRequestorCredentials", dto.StrongCustomerAuthentication.ThreeDSRequestorAuthenticationInfo.ThreeDSReqAuthMethod);
        Assert.Equal("201810011445", dto.StrongCustomerAuthentication.ThreeDSRequestorAuthenticationInfo.ThreeDSReqAuthTimestamp);

        Assert.NotNull(dto.StrongCustomerAuthentication.AcctInfo);
        Assert.Equal("MoreThanSixtyDays", dto.StrongCustomerAuthentication.AcctInfo!.ChAccAgeInd);
        Assert.Equal("20180925", dto.StrongCustomerAuthentication.AcctInfo.ChAccDate);
        Assert.Equal("NotSuspicious", dto.StrongCustomerAuthentication.AcctInfo.SuspiciousAccActivity);

        Assert.NotNull(dto.StrongCustomerAuthentication.MerchantRiskIndicator);
        Assert.Equal("shopper@example.com", dto.StrongCustomerAuthentication.MerchantRiskIndicator!.DeliveryEmailAddress);
        Assert.Equal("OvernightShipping", dto.StrongCustomerAuthentication.MerchantRiskIndicator.DeliveryTimeframe);
        Assert.Equal(DeliveryTimeframeIndicator.OvernightShipping, dto.StrongCustomerAuthentication.MerchantRiskIndicator.DeliveryTimeframeIndicator);
        Assert.Equal("123", dto.StrongCustomerAuthentication.MerchantRiskIndicator.GiftCardAmount);
        Assert.Equal("2", dto.StrongCustomerAuthentication.MerchantRiskIndicator.GiftCardCount);
        Assert.Equal("20200220", dto.StrongCustomerAuthentication.MerchantRiskIndicator.PreOrderDate);
        Assert.Equal("MerchandiseAvailable", dto.StrongCustomerAuthentication.MerchantRiskIndicator.PreOrderPurchaseInd);
        Assert.Equal(PreOrderPurchaseIndicator.MerchandiseAvailable, dto.StrongCustomerAuthentication.MerchantRiskIndicator.PreOrderPurchaseIndicator);
        Assert.Equal("Reordered", dto.StrongCustomerAuthentication.MerchantRiskIndicator.ReorderItemsInd);
        Assert.Equal(ReorderItemsIndicator.Reordered, dto.StrongCustomerAuthentication.MerchantRiskIndicator.ReorderItemsIndicator);
        Assert.Equal("CardholderBillingAddress", dto.StrongCustomerAuthentication.MerchantRiskIndicator.ShipIndicator);
        Assert.Equal(ShipIndicatorType.CardholderBillingAddress, dto.StrongCustomerAuthentication.MerchantRiskIndicator.ShipIndicatorType);

        Assert.NotNull(dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo);
        Assert.Equal("data", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo!.ThreeDSReqPriorAuthData);
        Assert.Equal("FrictionlessAuthentication", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo.ThreeDSReqPriorAuthMethod);
        Assert.Equal("201901011645", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo.ThreeDSReqPriorAuthTimestamp);
        Assert.Equal("2cd842f5-da5d-40b7-8ae6-6ce61cc7b580", dto.StrongCustomerAuthentication.ThreeDSRequestorPriorAuthenticationInfo.ThreeDSReqPriorRef);
    }

    /// <summary>
    /// Verifies that expanded payment payload fields are mapped to the DTO.
    /// </summary>
    [Fact(DisplayName = "CreateTransactionRequest WithExpandedPaymentFields MapsObject")]
    public void CreateTransactionRequest_WithExpandedPaymentFields_MapsObject()
    {
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = "Demo.Transaction-99",
            Amount = 567,
            Currency = "GBP",
            Description = "Demo Transaction",
            SettlementReferenceText = "123456GRTY234",
            EntryMethod = "Ecommerce",
            GiftAid = false,
            Apply3DSecure = Apply3DSecureOption.UseMSPSetting,
            ApplyAvsCvcCheck = "UseMSPSetting",
            CustomerEmail = "sam.jones@example.com",
            CustomerPhone = "+443069990210",
            CustomerMobilePhone = "+447234567891",
            CustomerWorkPhone = "+441234567891",
            ReferrerId = "f9979593-a390-4069-b126-7914783fc",
            BillingAddress = new BillingAddress
            {
                Address1 = "407 St. John Street",
                City = "London",
                PostalCode = "EC1V 4AB",
                Country = "GB",
                State = "st"
            },
            ShippingDetails = new ShippingDetails
            {
                RecipientFirstName = "Sam",
                RecipientLastName = "Jones",
                ShippingAddress1 = "407 St. John Street",
                ShippingCity = "London",
                ShippingPostalCode = "EC1V 4AB",
                ShippingCountry = "GB",
                ShippingState = "st"
            },
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = "90BDF208-3C19-40AC-858B-3F4054DCD1C0",
                    CardIdentifier = "7F3CCA38-0BDA-453B-BF62-9CBD5891F77E",
                    Reusable = false,
                    Save = false
                },
                PayPal = new PayPalPaymentMethod
                {
                    MerchantSessionKey = "90BDF208-3C19-40AC-858B-3F4054DCD1C0",
                    CallbackUrl = "https://www.example.com"
                },
                ApplePay = new ApplePayPaymentMethod
                {
                    PaymentData = "AAAAAAABBBBBCCCCCC",
                    ClientIpAddress = "10.20.30.40",
                    MerchantSessionKey = "90BDF208-3C19-40AC-858B-3F4054DCD1C0",
                    SessionValidationToken = "SFGVHSBEVGAV/VDAYRR+345S",
                    ApplicationData = "FOeVKLA...PFE4wrw==",
                    DisplayName = "Visa 1234",
                    PaymentMethodType = "Debit"
                },
                GooglePay = new GooglePayPaymentMethod
                {
                    Payload = "AAAAAAABBBBBCCCCCC",
                    ClientIpAddress = "10.20.30.40",
                    MerchantSessionKey = "90BDF208-3C19-40AC-858B-3F4054DCD1C0"
                }
            },
            CredentialType = new CredentialType
            {
                CofUsage = "First",
                InitiatedType = "CIT",
                MitType = "Unscheduled",
                RecurringExpiry = "20200301",
                RecurringFrequency = "28",
                PurchaseInstalData = "6"
            },
            FiRecipient = new FiRecipientRequest
            {
                AccountNumber = "1234567890",
                Surname = "Surname",
                Postcode = "EC1V 8AB",
                DateOfBirth = "19900101"
            },
            AccountFunding = new AccountFundingRequest
            {
                Sender = new AccountFundingParty
                {
                    FirstName = "string",
                    LastName = "string",
                    City = "12345",
                    State = "st",
                    Country = "st"
                },
                Recipient = new AccountFundingParty
                {
                    FirstName = "string",
                    LastName = "string",
                    City = "12345",
                    State = "st",
                    Country = "string",
                    AccountNumber = "string",
                    TransactionReference = "string"
                },
                PaymentProgramIndicator = "string"
            }
        };

        var dto = RequestMapper.ToDto(request);

        Assert.Equal("123456GRTY234", dto.SettlementReferenceText);
        Assert.Equal("Ecommerce", dto.EntryMethod);
        Assert.False(dto.GiftAid);
        Assert.Equal("UseMSPSetting", dto.Apply3DSecure);
        Assert.Equal("UseMSPSetting", dto.ApplyAvsCvcCheck);
        Assert.Equal("sam.jones@example.com", dto.CustomerEmail);
        Assert.Equal("+443069990210", dto.CustomerPhone);
        Assert.Equal("+447234567891", dto.CustomerMobilePhone);
        Assert.Equal("+441234567891", dto.CustomerWorkPhone);
        Assert.Equal("f9979593-a390-4069-b126-7914783fc", dto.ReferrerId);

        Assert.NotNull(dto.BillingAddress);
        Assert.Equal("st", dto.BillingAddress!.State);

        Assert.NotNull(dto.ShippingDetails);
        Assert.Equal("Sam", dto.ShippingDetails!.RecipientFirstName);
        Assert.Equal("st", dto.ShippingDetails.ShippingState);

        Assert.NotNull(dto.PaymentMethod);
        Assert.NotNull(dto.PaymentMethod!.Card);
        Assert.False(dto.PaymentMethod.Card!.Reusable);
        Assert.False(dto.PaymentMethod.Card.Save);
        Assert.NotNull(dto.PaymentMethod.PayPal);
        Assert.Equal("https://www.example.com", dto.PaymentMethod.PayPal!.CallbackUrl);
        Assert.NotNull(dto.PaymentMethod.ApplePay);
        Assert.Equal("Debit", dto.PaymentMethod.ApplePay!.PaymentMethodType);
        Assert.NotNull(dto.PaymentMethod.GooglePay);
        Assert.Equal("10.20.30.40", dto.PaymentMethod.GooglePay!.ClientIpAddress);

        Assert.NotNull(dto.CredentialType);
        Assert.Equal("First", dto.CredentialType!.CofUsage);
        Assert.Equal("Unscheduled", dto.CredentialType.MitType);

        Assert.NotNull(dto.FiRecipient);
        Assert.Equal("1234567890", dto.FiRecipient!.AccountNumber);

        Assert.NotNull(dto.AccountFunding);
        Assert.Equal("string", dto.AccountFunding!.PaymentProgramIndicator);
        Assert.NotNull(dto.AccountFunding.Sender);
        Assert.Equal("st", dto.AccountFunding.Sender!.State);
        Assert.NotNull(dto.AccountFunding.Recipient);
        Assert.Equal("string", dto.AccountFunding.Recipient!.AccountNumber);
    }
}
