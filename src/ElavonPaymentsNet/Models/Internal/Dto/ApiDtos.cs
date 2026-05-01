using ElavonPaymentsNet.Models.Public;

namespace ElavonPaymentsNet.Models.Internal.Dto;

// ---------------------------------------------------------------------------
// Transaction creation request
// Required because the API expects a string literal for transactionType,
// which cannot be driven directly from the TransactionType enum with the
// global JsonStringEnumConverter (which lowercases enum names).
// ---------------------------------------------------------------------------

internal sealed class CreateTransactionRequestDto
{
    public required string TransactionType { get; init; }
    public required string VendorTxCode { get; init; }
    public required int Amount { get; init; }
    public required string Currency { get; init; }
    public string? Description { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public BillingAddress? BillingAddress { get; init; }
    public string? CustomerEmail { get; init; }
    public string? CustomerFirstName { get; init; }
    public string? CustomerLastName { get; init; }
    public string? RelatedTransactionId { get; init; }
}

// ---------------------------------------------------------------------------
// Token payment request
// Required because the public PayWithTokenRequest has a flat Token property
// that must be wrapped into paymentMethod.token for the API wire format,
// and transactionType must be injected.
// ---------------------------------------------------------------------------

internal sealed class PayWithTokenRequestDto
{
    public required string TransactionType { get; init; }
    public required string VendorTxCode { get; init; }
    public required int Amount { get; init; }
    public required string Currency { get; init; }
    public required PaymentMethod PaymentMethod { get; init; }
}
