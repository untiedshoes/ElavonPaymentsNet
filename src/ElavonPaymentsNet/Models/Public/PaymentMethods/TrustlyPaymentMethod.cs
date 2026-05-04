namespace ElavonPaymentsNet.Models.Public;

/// <summary>Trustly payment details.</summary>
public sealed class TrustlyPaymentMethod
{
    /// <summary>Merchant session key associated with the payment.</summary>
    public string? MerchantSessionKey { get; init; }

    /// <summary>Callback URL for post-authorization return.</summary>
    public string? CallbackUrl { get; init; }

    /// <summary>Language code used for the payment journey.</summary>
    public string? LanguageCode { get; init; }

    /// <summary>Client IP address of the shopper.</summary>
    public string? ClientIpAddress { get; init; }

    /// <summary>Beneficiary identifier for payout flows.</summary>
    public string? BeneficiaryId { get; init; }

    /// <summary>Beneficiary name for payout flows.</summary>
    public string? BeneficiaryName { get; init; }

    /// <summary>Beneficiary address for payout flows.</summary>
    public string? BeneficiaryAddress { get; init; }

    /// <summary>Beneficiary country code for payout flows.</summary>
    public string? BeneficiaryCountryCode { get; init; }
}
