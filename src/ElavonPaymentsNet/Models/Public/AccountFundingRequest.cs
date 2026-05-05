namespace ElavonPaymentsNet.Models.Public;

/// <summary>Account funding details for sender/recipient payment program flows.</summary>
public sealed class AccountFundingRequest
{
    /// <summary>Sender party details.</summary>
    public AccountFundingParty? Sender { get; init; }

    /// <summary>Recipient party details.</summary>
    public AccountFundingParty? Recipient { get; init; }

    /// <summary>Scheme/acquirer payment program indicator.</summary>
    public string? PaymentProgramIndicator { get; init; }
}

/// <summary>Sender/recipient party details used in account-funding requests.</summary>
public sealed class AccountFundingParty
{
    /// <summary>Party first name.</summary>
    public string? FirstName { get; init; }

    /// <summary>Party middle name.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Party last name/surname.</summary>
    public string? LastName { get; init; }

    /// <summary>Street address.</summary>
    public string? Address { get; init; }

    /// <summary>City or locality.</summary>
    public string? City { get; init; }

    /// <summary>State or province.</summary>
    public string? State { get; init; }

    /// <summary>Postal or ZIP code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Country code/value expected by the acquirer.</summary>
    public string? Country { get; init; }

    /// <summary>Date of birth in the format required by the acquirer/program.</summary>
    public string? DateOfBirth { get; init; }

    /// <summary>Purpose-of-payment code where mandated by scheme rules.</summary>
    public string? PurposeOfPaymentCode { get; init; }

    /// <summary>Funding account number.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Funding account type.</summary>
    public string? AccountType { get; init; }

    /// <summary>Merchant-side reference for reconciliation.</summary>
    public string? TransactionReference { get; init; }
}
