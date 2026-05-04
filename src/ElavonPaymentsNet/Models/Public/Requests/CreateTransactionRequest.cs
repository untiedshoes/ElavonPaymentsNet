namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>
/// Request model for creating any type of payment transaction.
/// The <see cref="TransactionType"/> property determines the operation performed.
/// </summary>
public class CreateTransactionRequest
{
    /// <summary>The type of transaction to create.</summary>
    public required TransactionType TransactionType { get; init; }

    /// <summary>A unique vendor-assigned transaction code.</summary>
    public required string VendorTxCode { get; init; }

    /// <summary>Amount in the smallest currency unit (e.g. pence for GBP).</summary>
    public required int Amount { get; init; }

    /// <summary>ISO 4217 currency code, e.g. "GBP".</summary>
    public required string Currency { get; init; }

    /// <summary>A short description of the transaction. Recommended for <see cref="TransactionType.Payment"/>.</summary>
    public string? Description { get; init; }

    /// <summary>Settlement reference text shown in downstream acquirer reporting where supported.</summary>
    public string? SettlementReferenceText { get; init; }

    /// <summary>Payment method containing card details or a token. Not required for <see cref="TransactionType.Repeat"/>.</summary>
    public PaymentMethod? PaymentMethod { get; init; }

    /// <summary>Billing address for the transaction.</summary>
    public BillingAddress? BillingAddress { get; init; }

    /// <summary>Customer's email address.</summary>
    public string? CustomerEmail { get; init; }

    /// <summary>Customer phone number.</summary>
    public string? CustomerPhone { get; init; }

    /// <summary>Customer mobile phone number.</summary>
    public string? CustomerMobilePhone { get; init; }

    /// <summary>Customer work phone number.</summary>
    public string? CustomerWorkPhone { get; init; }

    /// <summary>Customer first name.</summary>
    public string? CustomerFirstName { get; init; }

    /// <summary>Customer last name.</summary>
    public string? CustomerLastName { get; init; }

    /// <summary>
    /// Controls whether 3D Secure authentication is applied, overriding the account-level setting.
    /// Set to <see cref="Apply3DSecureOption.Disable"/> to bypass 3DS for this transaction.
    /// </summary>
    public Apply3DSecureOption? Apply3DSecure { get; init; }

    /// <summary>Controls AVS/CVC checks for this transaction (gateway-specific values).</summary>
    public string? ApplyAvsCvcCheck { get; init; }

    /// <summary>Entry method indicator (for example, <c>Ecommerce</c>).</summary>
    public string? EntryMethod { get; init; }

    /// <summary>Gift Aid flag (typically UK charity flows).</summary>
    public bool? GiftAid { get; init; }

    /// <summary>Shipping recipient and address details.</summary>
    public ShippingDetails? ShippingDetails { get; init; }

    /// <summary>Integrator or referrer identifier.</summary>
    public string? ReferrerId { get; init; }

    /// <summary>
    /// The transaction ID of the original payment to reference.
    /// Required when <see cref="TransactionType"/> is <see cref="TransactionType.Repeat"/>
    /// or <see cref="TransactionType.Refund"/>.
    /// </summary>
    public string? RelatedTransactionId { get; init; }

    /// <summary>
    /// Browser and device data required for EMV 3DS v2 (Strong Customer Authentication).
    /// Should be included on all Payment and Authorise transactions where 3DS may be
    /// triggered, to allow the issuer to perform a frictionless or challenge flow.
    /// </summary>
    public StrongCustomerAuthentication? StrongCustomerAuthentication { get; init; }

    /// <summary>Credential-on-file metadata for CIT/MIT/recurring scenarios.</summary>
    public CredentialType? CredentialType { get; init; }

    /// <summary>Financial institution recipient details for applicable MCC/payment programs.</summary>
    public FiRecipientRequest? FiRecipient { get; init; }

    /// <summary>Account-funding details for sender/recipient where required by scheme rules.</summary>
    public AccountFundingRequest? AccountFunding { get; init; }
}
