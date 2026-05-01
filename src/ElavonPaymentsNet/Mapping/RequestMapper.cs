using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public;
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
            PaymentMethod = r.PaymentMethod,
            BillingAddress = r.BillingAddress,
            CustomerEmail = r.CustomerEmail,
            CustomerFirstName = r.CustomerFirstName,
            CustomerLastName = r.CustomerLastName,
            Apply3DSecure = r.Apply3DSecure?.ToString(),
            RelatedTransactionId = r.RelatedTransactionId
        };

    internal static PayWithTokenRequestDto ToDto(PayWithTokenRequest r) =>
        new()
        {
            TransactionType = "Payment",
            VendorTxCode = r.VendorTxCode,
            Amount = r.Amount,
            Currency = r.Currency,
            PaymentMethod = new PaymentMethod { Token = r.Token }
        };
}
