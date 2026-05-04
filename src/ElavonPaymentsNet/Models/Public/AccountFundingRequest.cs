namespace ElavonPaymentsNet.Models.Public;

/// <summary>Account funding details for sender/recipient payment program flows.</summary>
public sealed class AccountFundingRequest
{
    public AccountFundingParty? Sender { get; init; }
    public AccountFundingParty? Recipient { get; init; }
    public string? PaymentProgramIndicator { get; init; }
}

/// <summary>Sender/recipient party details used in account-funding requests.</summary>
public sealed class AccountFundingParty
{
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PurposeOfPaymentCode { get; init; }
    public string? AccountNumber { get; init; }
    public string? AccountType { get; init; }
    public string? TransactionReference { get; init; }
}
