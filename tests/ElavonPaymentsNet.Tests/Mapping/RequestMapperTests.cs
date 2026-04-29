using ElavonPaymentsNet.Mapping;
using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public;

namespace ElavonPaymentsNet.Tests.Mapping;

public class RequestMapperTests
{
    /// <summary>
    /// Verifies that a payment transaction request maps all core fields and card details.
    /// </summary>
    [Fact]
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
    [Fact]
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
    [Fact]
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
    /// Verifies that a repeat transaction maps the related transaction identifier.
    /// </summary>
    [Fact]
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
        Assert.Equal("original-tx-id", dto.RelatedTransactionId);
    }

    /// <summary>
    /// Verifies that billing address fields are mapped correctly to the DTO.
    /// </summary>
    [Fact]
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
    [Fact]
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
}
