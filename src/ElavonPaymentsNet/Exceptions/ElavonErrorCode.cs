namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Named constants for the Elavon/Opayo application-level error codes returned in
/// <see cref="ElavonApiException.ErrorCode"/>.
///
/// These codes are included in API error responses alongside the HTTP status code and
/// provide the machine-readable reason for the failure. Use them in exception filters to
/// give your application precise, actionable error paths without relying on magic strings.
///
/// <example>
/// <code>
/// catch (ElavonValidationException ex) when (ex.ErrorCode == ElavonErrorCode.RefundExceedsOriginal)
/// {
///     // Amount entered by customer exceeds what was originally charged.
/// }
/// catch (ElavonValidationException ex) when (ex.ErrorCode == ElavonErrorCode.DuplicateVendorTxCode)
/// {
///     // Retry with a unique VendorTxCode.
/// }
/// </code>
/// </example>
/// </summary>
public static class ElavonErrorCode
{
    // -------------------------------------------------------------------------
    // Gateway / system errors (2xxx) — typically ElavonServerException
    // -------------------------------------------------------------------------

    /// <summary>
    /// A system error has occurred. This often indicates a misconfiguration of your
    /// Merchant ID with the acquirer, a temporary comms failure between Elavon and a
    /// third party, or an internal platform fault.
    /// If it persists, contact opayosupport@elavon.com.
    /// Typically paired with HTTP 5xx (<see cref="ElavonServerException"/>).
    /// </summary>
    public const string SystemError = "2003";

    /// <summary>
    /// The server encountered an unexpected internal error and could not fulfil the request.
    /// Check your MyOpayo dashboard to confirm the transaction status before retrying.
    /// Typically paired with HTTP 5xx (<see cref="ElavonServerException"/>).
    /// </summary>
    public const string InternalServerError = "2015";

    /// <summary>
    /// The transaction was declined by the bank because Strong Customer Authentication (SCA)
    /// is required. Ensure you are sending a complete <c>strongCustomerAuthentication</c> object
    /// including the shopper IP in <c>clientIP</c>.
    /// Typically paired with HTTP 402 (<see cref="ElavonPaymentDeclinedException"/>).
    /// </summary>
    public const string ScaRequired = "2022";

    // -------------------------------------------------------------------------
    // Account / card-range restrictions (3xxx) — typically ElavonValidationException
    // -------------------------------------------------------------------------

    /// <summary>
    /// The card type used by the customer is not enabled on your Elavon account.
    /// All Visa and Mastercard are enabled by default; other types (e.g. Amex, Diners, JCB)
    /// must be specifically activated. Ensure your payment page clearly communicates
    /// accepted card types.
    /// </summary>
    public const string PaymentSystemNotSupported = "3069";

    /// <summary>
    /// The <c>BillingState</c> value submitted exceeds 2 characters (ANSI state code).
    /// Review the validation on your payment form.
    /// Available in <see cref="ElavonValidationException.ValidationErrors"/> for the exact field.
    /// </summary>
    public const string BillingStateTooLong = "3130";

    /// <summary>
    /// 3D Secure authentication was rejected by the card issuer — most likely because
    /// the cardholder entered incorrect 3DS credentials.
    /// The customer should retry with the correct credentials or use a different card.
    /// </summary>
    public const string ThreeDsRejectedByIssuer = "3336";

    // -------------------------------------------------------------------------
    // Transaction / business-rule errors (4xxx) — typically ElavonValidationException
    // -------------------------------------------------------------------------

    /// <summary>
    /// The <c>VendorTxCode</c> has already been used for a previous transaction.
    /// Every transaction registration POST must use a unique <c>VendorTxCode</c>.
    /// This can occur if a customer resubmits during 3DSv2 authentication.
    /// Generate a fresh code before retrying.
    /// Also applies to error codes 4002 and 4042.
    /// </summary>
    public const string DuplicateVendorTxCode = "4001";

    /// <summary>
    /// The amount submitted is invalid — either it exceeds the Elavon configured maximum
    /// for the currency, or it is zero for a transaction type that does not permit it
    /// (zero is only valid for <c>Authenticate</c>).
    /// </summary>
    public const string AmountOutOfRange = "4009";

    /// <summary>
    /// The client IP address supplied in <c>strongCustomerAuthentication.clientIP</c>
    /// has been blocked on your Elavon account (via MyOpayo Settings &gt; Restrictions).
    /// This is typically a fraud-prevention measure. Review your IP block list in MyOpayo.
    /// </summary>
    public const string ClientIpRestricted = "4019";

    /// <summary>
    /// 3D Secure authentication failed and your account rules require a successful
    /// 3DS result before authorisation. Ensure you are sending the full
    /// <c>strongCustomerAuthentication</c> object. Do not disable 3DS rules as a workaround.
    /// </summary>
    public const string ThreeDsRulesRequireAuth = "4026";

    /// <summary>
    /// 3D Secure authentication failed and the card cannot be authorised.
    /// Your account has no 3DS override rules configured.
    /// The customer should retry with a different card or contact their bank.
    /// </summary>
    public const string ThreeDsCannotAuthoriseCard = "4027";

    /// <summary>
    /// The refund amount exceeds the value of the original transaction.
    /// Ensure you cap the refund at the original captured amount.
    /// Use the retrieve transaction endpoint to confirm the original value if needed.
    /// </summary>
    public const string RefundExceedsOriginal = "4035";

    /// <summary>
    /// The card range used in this transaction has been blocked by your account rule base.
    /// Review the Restrictions page under Settings in MyOpayo.
    /// </summary>
    public const string CardRangeBlocked = "4043";

    /// <summary>
    /// The Authorise amount exceeds 115% of the original transaction amount, which is
    /// the maximum permitted uplift. Reduce the authorise amount accordingly.
    /// </summary>
    public const string AuthoriseExceedsMaximum = "4044";

    /// <summary>
    /// No card details are registered for the supplied token or card identifier.
    /// This can occur if the identifier is invalid, has been removed from Elavon's systems,
    /// or if you have accidentally submitted a <c>merchantSessionKey</c> or
    /// <c>transactionId</c> instead of the card identifier.
    /// </summary>
    public const string TokenNotFound = "4057";

    // -------------------------------------------------------------------------
    // Field-level validation errors (5xxx) — typically ElavonValidationException
    // -------------------------------------------------------------------------

    /// <summary>
    /// A postcode field contains unsupported characters.
    /// Accepted characters are letters, digits, hyphens, and spaces.
    /// Review the postcode validation on your integration.
    /// </summary>
    public const string InvalidPostcodeCharacters = "5055";

    /// <summary>
    /// An unexpected <c>CRes</c> was received from the 3D Secure provider.
    /// This indicates a communication failure between the ACS and Elavon.
    /// The transaction has failed and must be re-attempted from the beginning
    /// with a new <c>VendorTxCode</c>.
    /// </summary>
    public const string UnexpectedCRes = "5086";

    // -------------------------------------------------------------------------
    // Apple Pay errors (6xxx) — typically ElavonValidationException
    // -------------------------------------------------------------------------

    /// <summary>
    /// The Apple Pay payload could not be decoded because it is not valid Base64.
    /// Ensure the payload is Base64-encoded before submission.
    /// </summary>
    public const string ApplePayInvalidBase64 = "6111";

    /// <summary>
    /// The Apple Pay payload is Base64-encoded correctly but the decoded content is
    /// not valid JSON, or the JSON was double-encoded before Base64 encoding.
    /// Encode only the raw JSON payload, not a pre-encoded string.
    /// </summary>
    public const string ApplePayInvalidPayloadFormat = "6112";

    /// <summary>
    /// The Apple Pay payload is missing the required TAVV (Token Authentication
    /// Verification Value). Ensure that <c>supportsEMV</c> is not present in the
    /// <c>merchantCapabilities</c> array in your Apple Pay JS button configuration.
    /// </summary>
    public const string ApplePayTavvRequired = "6116";

    /// <summary>
    /// The Apple Pay <c>token</c> property was not supplied in the request body.
    /// The entire Apple Pay payload JSON object must be wrapped inside a JSON
    /// object named <c>token</c> before Base64-encoding.
    /// </summary>
    public const string ApplePayTokenNotSupplied = "6149";

    /// <summary>
    /// The domain used in the Apple Pay merchant session request is not registered
    /// in MyOpayo. Ensure your domain verification file is correctly uploaded and
    /// that the registered domain exactly matches the one in the session request
    /// (e.g. <c>www.example.com</c> and <c>example.com</c> are treated as distinct).
    /// </summary>
    public const string ApplePayDomainNotRegistered = "6118";
}
