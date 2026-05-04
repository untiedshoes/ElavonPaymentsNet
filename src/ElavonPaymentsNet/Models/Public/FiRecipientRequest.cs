namespace ElavonPaymentsNet.Models.Public;

/// <summary>Financial institution recipient details for specific MCC/payment program flows.</summary>
public sealed class FiRecipientRequest
{
    public string? AccountNumber { get; init; }
    public string? Surname { get; init; }
    public string? Postcode { get; init; }
    public string? DateOfBirth { get; init; }
}
