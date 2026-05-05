namespace ElavonPaymentsNet.Models.Public;

/// <summary>Credential-on-file metadata for CIT/MIT/recurring transactions.</summary>
public sealed class CredentialType
{
    /// <summary>Credential-on-file usage indicator, for example First or Subsequent.</summary>
    public string? CofUsage { get; init; }

    /// <summary>Initiator type, typically CIT (cardholder initiated) or MIT (merchant initiated).</summary>
    public string? InitiatedType { get; init; }

    /// <summary>MIT subtype (for example Unscheduled, Recurring, or Instalment).</summary>
    public string? MitType { get; init; }

    /// <summary>Recurring agreement expiry (format as required by gateway/scheme).</summary>
    public string? RecurringExpiry { get; init; }

    /// <summary>Recurring frequency interval value when applicable.</summary>
    public string? RecurringFrequency { get; init; }

    /// <summary>Installment data indicator/value for installment payment agreements.</summary>
    public string? PurchaseInstalData { get; init; }
}
