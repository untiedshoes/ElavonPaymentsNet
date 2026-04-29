using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public.Requests;

namespace ElavonPaymentsNet.Mapping;

/// <summary>
/// Maps public SDK request models to internal API DTOs where the wire format
/// cannot be produced by direct serialisation. Used only where a
/// transactionType string literal must be injected into the request.
/// </summary>
internal static class RequestMapper
{
    internal static CreateTransactionRequestDto ToDto(CreateTransactionRequest r) =>
        new()
        {
            TransactionType = r.TransactionType.ToString(),
            VendorTxCode = r.VendorTxCode,
            Amount = r.Amount,
            Currency = r.Currency,
            Description = r.Description,
            PaymentMethod = r.PaymentMethod is null
                ? new PaymentMethodDto()
                : new PaymentMethodDto { Card = r.PaymentMethod.Card, Token = r.PaymentMethod.Token },
            BillingAddress = r.BillingAddress,
            CustomerEmail = r.CustomerEmail,
            RelatedTransactionId = r.RelatedTransactionId
        };

    internal static PayWithTokenRequestDto ToDto(PayWithTokenRequest r) =>
        new()
        {
            TransactionType = "Payment",
            VendorTxCode = r.VendorTxCode,
            Amount = r.Amount,
            Currency = r.Currency,
            PaymentMethod = new PaymentMethodDto { Token = r.Token }
        };
}
