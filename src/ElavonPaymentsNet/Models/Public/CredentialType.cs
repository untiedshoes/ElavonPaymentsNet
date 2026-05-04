namespace ElavonPaymentsNet.Models.Public;

/// <summary>Credential-on-file metadata for CIT/MIT/recurring transactions.</summary>
public sealed class CredentialType
{
    public string? CofUsage { get; init; }
    public string? InitiatedType { get; init; }
    public string? MitType { get; init; }
    public string? RecurringExpiry { get; init; }
    public string? RecurringFrequency { get; init; }
    public string? PurchaseInstalData { get; init; }
}
