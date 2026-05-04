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
            SettlementReferenceText = isRepeat ? null : r.SettlementReferenceText,
            PaymentMethod = isRepeat ? null : r.PaymentMethod,
            BillingAddress = isRepeat ? null : r.BillingAddress,
            CustomerEmail = isRepeat ? null : r.CustomerEmail,
            CustomerPhone = isRepeat ? null : r.CustomerPhone,
            CustomerMobilePhone = isRepeat ? null : r.CustomerMobilePhone,
            CustomerWorkPhone = isRepeat ? null : r.CustomerWorkPhone,
            CustomerFirstName = isRepeat ? null : r.CustomerFirstName,
            CustomerLastName = isRepeat ? null : r.CustomerLastName,
            EntryMethod = isRepeat ? null : r.EntryMethod,
            GiftAid = isRepeat ? null : r.GiftAid,
            Apply3DSecure = isRepeat ? null : r.Apply3DSecure?.ToString(),
            ApplyAvsCvcCheck = isRepeat ? null : r.ApplyAvsCvcCheck,
            ShippingDetails = isRepeat ? null : r.ShippingDetails,
            ReferrerId = isRepeat ? null : r.ReferrerId,
            ReferenceTransactionId = r.RelatedTransactionId,
            StrongCustomerAuthentication = isRepeat ? null : r.StrongCustomerAuthentication,
            CredentialType = isRepeat ? null : r.CredentialType,
            FiRecipient = isRepeat ? null : r.FiRecipient,
            AccountFunding = isRepeat ? null : r.AccountFunding
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
