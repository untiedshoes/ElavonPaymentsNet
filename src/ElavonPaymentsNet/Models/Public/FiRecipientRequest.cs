namespace ElavonPaymentsNet.Models.Public;

/// <summary>Financial institution recipient details for specific MCC/payment program flows.</summary>
public sealed class FiRecipientRequest
{
    /// <summary>Recipient account number.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Recipient surname.</summary>
    public string? Surname { get; init; }

    /// <summary>Recipient postal or ZIP code.</summary>
    public string? Postcode { get; init; }

    /// <summary>Recipient date of birth (format depends on acquirer rules).</summary>
    public string? DateOfBirth { get; init; }
}
