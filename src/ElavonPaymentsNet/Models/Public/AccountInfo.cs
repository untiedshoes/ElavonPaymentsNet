namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Cardholder account information used by EMV 3DS risk engines.
/// </summary>
public sealed class AccountInfo
{
    /// <summary>Length of time the account has existed.</summary>
    public string? ChAccAgeInd { get; init; }

    /// <summary>Date when account details were last changed (YYYYMMDD).</summary>
    public string? ChAccChange { get; init; }

    /// <summary>Length of time since account details were changed.</summary>
    public string? ChAccChangeInd { get; init; }

    /// <summary>Account creation date (YYYYMMDD).</summary>
    public string? ChAccDate { get; init; }

    /// <summary>Date when password was last changed (YYYYMMDD).</summary>
    public string? ChAccPwChange { get; init; }

    /// <summary>Length of time since password was changed.</summary>
    public string? ChAccPwChangeInd { get; init; }

    /// <summary>Number of purchases on this account in the last six months.</summary>
    public string? NbPurchaseAccount { get; init; }

    /// <summary>Number of add-card attempts in the last day.</summary>
    public string? ProvisionAttemptsDay { get; init; }

    /// <summary>Number of transactions in the last day.</summary>
    public string? TxnActivityDay { get; init; }

    /// <summary>Number of transactions in the last year.</summary>
    public string? TxnActivityYear { get; init; }

    /// <summary>Date when the payment account was enrolled (YYYYMMDD).</summary>
    public string? PaymentAccAge { get; init; }

    /// <summary>Length of time the payment account has existed.</summary>
    public string? PaymentAccInd { get; init; }

    /// <summary>Date when shipping address was first used (YYYYMMDD).</summary>
    public string? ShipAddressUsage { get; init; }

    /// <summary>Length of time shipping address has been used.</summary>
    public string? ShipAddressUsageInd { get; init; }

    /// <summary>Whether cardholder name matches shipping recipient name.</summary>
    public string? ShipNameIndicator { get; init; }

    /// <summary>Suspicious activity indicator for the account.</summary>
    public string? SuspiciousAccActivity { get; init; }
}
