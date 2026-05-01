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
    internal static CreateTransactionRequestDto ToDto(CreateTransactionRequest r)
    {
        var isRepeat = r.TransactionType == TransactionType.Repeat;

        return new()
        {
            TransactionType = r.TransactionType.ToString(),
            VendorTxCode = r.VendorTxCode,
            Amount = r.Amount,
            Currency = r.Currency,
            Description = r.Description,
            PaymentMethod = isRepeat ? null : r.PaymentMethod,
            BillingAddress = isRepeat ? null : r.BillingAddress,
            CustomerEmail = isRepeat ? null : r.CustomerEmail,
            CustomerFirstName = isRepeat ? null : r.CustomerFirstName,
            CustomerLastName = isRepeat ? null : r.CustomerLastName,
            Apply3DSecure = isRepeat ? null : r.Apply3DSecure?.ToString(),
            ReferenceTransactionId = r.RelatedTransactionId
        };
    }

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
