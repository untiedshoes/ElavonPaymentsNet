namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Known indicator values for 3DS exemption requests.
/// </summary>
public enum ThreeDSExemptionIndicatorType
{
    /// <summary>Low-value exemption request.</summary>
    LowValue,

    /// <summary>Transaction risk analysis exemption request.</summary>
    TransactionRiskAnalysis,

    /// <summary>Trusted beneficiary exemption request.</summary>
    TrustedBeneficiary,

    /// <summary>Secure corporate payment exemption request.</summary>
    SecureCorporatePayment
}
