using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Browser and device data required for EMV 3DS v2 (Strong Customer Authentication).
/// Must be included on Payment and Authorise transactions when 3D Secure authentication
/// is required or may be triggered, so that the issuer can perform a risk-based assessment
/// and optionally issue a challenge.
/// </summary>
public sealed class StrongCustomerAuthentication
{
    private readonly string? _threeDSExemptionIndicator;

    /// <summary>
    /// Fully qualified URL that receives the challenge result callback from the ACS.
    /// </summary>
    public string? NotificationURL { get; init; }

    /// <summary>The IP address of the cardholder's browser.</summary>
    public string? BrowserIP { get; init; }

    /// <summary>
    /// The Accept header sent by the cardholder's browser.
    /// e.g. <c>text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8</c>
    /// </summary>
    public string? BrowserAcceptHeader { get; init; }

    /// <summary>Whether JavaScript is enabled in the cardholder's browser.</summary>
    public bool? BrowserJavascriptEnabled { get; init; }

    /// <summary>Whether Java is enabled in the cardholder's browser.</summary>
    public bool? BrowserJavaEnabled { get; init; }

    /// <summary>The BCP 47 language tag reported by the browser. e.g. <c>en-GB</c>.</summary>
    public string? BrowserLanguage { get; init; }

    /// <summary>The colour depth of the cardholder's screen in bits. e.g. <c>24</c>.</summary>
    public string? BrowserColorDepth { get; init; }

    /// <summary>The height of the cardholder's screen in pixels. e.g. <c>1080</c>.</summary>
    public string? BrowserScreenHeight { get; init; }

    /// <summary>The width of the cardholder's screen in pixels. e.g. <c>1920</c>.</summary>
    public string? BrowserScreenWidth { get; init; }

    /// <summary>
    /// The time-zone offset of the cardholder's browser from UTC, in minutes.
    /// e.g. <c>0</c> for UTC, <c>-60</c> for UTC+1.
    /// </summary>
    public string? BrowserTZ { get; init; }

    /// <summary>The User-Agent string of the cardholder's browser.</summary>
    public string? BrowserUserAgent { get; init; }

    /// <summary>
    /// The size of the challenge window displayed to the cardholder.
    /// Valid values: <c>Small</c>, <c>Medium</c>, <c>Large</c>, <c>ExtraLarge</c>, <c>FullScreen</c>.
    /// Defaults to <c>Medium</c> if not provided.
    /// </summary>
    public string? ChallengeWindowSize { get; init; }

    /// <summary>
    /// Indicates the type of transaction being authenticated.
    /// Valid values: <c>GoodsAndServicePurchase</c>, <c>CheckAcceptance</c>,
    /// <c>AccountFunding</c>, <c>QuasiCashTransaction</c>, <c>PrepaidActivationAndLoad</c>.
    /// Defaults to <c>GoodsAndServicePurchase</c>.
    /// </summary>
    public string? TransType { get; init; }

    /// <summary>
    /// Indicates the 3DS Requestor's preference for whether a challenge should be performed.
    /// Valid values: <c>01</c> (No Preference), <c>02</c> (No Challenge Requested),
    /// <c>03</c> (Challenge Requested — Preference), <c>04</c> (Challenge Requested — Mandate).
    /// </summary>
    public string? ThreeDSRequestorChallengeInd { get; init; }

    /// <summary>
    /// Account identifier available to the merchant for this cardholder.
    /// </summary>
    public string? AcctID { get; init; }

    /// <summary>
    /// Details about how the cardholder was authenticated by the 3DS requestor.
    /// </summary>
    public ThreeDSRequestorAuthenticationInfo? ThreeDSRequestorAuthenticationInfo { get; init; }

    /// <summary>
    /// Cardholder account lifecycle and activity information used by issuer risk engines.
    /// </summary>
    public AccountInfo? AcctInfo { get; init; }

    /// <summary>
    /// Merchant risk signals sent to the issuer to support frictionless authentication decisions.
    /// Populate this for higher acceptance on low-risk transactions and better issuer scoring.
    /// </summary>
    public MerchantRiskIndicator? MerchantRiskIndicator { get; init; }

    /// <summary>
    /// Information about prior cardholder authentication for this shopper/session.
    /// This can help the issuer evaluate whether step-up challenge is necessary.
    /// </summary>
    public ThreeDSRequestorPriorAuthenticationInfo? ThreeDSRequestorPriorAuthenticationInfo { get; init; }

    /// <summary>
    /// Indicates which SCA exemption is being requested, if any.
    /// Values are acquirer/gateway specific; use the exact code expected by your Elavon configuration.
    /// </summary>
    [JsonPropertyName("threeDSExemptionIndicator")]
    public string? ThreeDSExemptionIndicator
    {
        get => _threeDSExemptionIndicator;
        init => _threeDSExemptionIndicator = value;
    }

    /// <summary>
    /// Backward-compatible alias for <see cref="ThreeDSExemptionIndicator"/>.
    /// </summary>
    [JsonIgnore]
    public string? ThreeDSRequestorExemptionIndicator
    {
        get => _threeDSExemptionIndicator;
        init => _threeDSExemptionIndicator = value;
    }

    /// <summary>
    /// Merchant website URL associated with the checkout flow.
    /// </summary>
    public string? Website { get; init; }
}
