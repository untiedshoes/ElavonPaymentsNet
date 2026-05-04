namespace ElavonPaymentsNet.Http;

/// <summary>
/// Centralized relative API routes used by service implementations.
/// </summary>
internal static class ElavonApiRoutes
{
    /// <summary>Base route for transaction creation requests.</summary>
    internal const string Transactions = "/transactions";

    /// <summary>Route for card tokenization requests.</summary>
    internal const string Token = "/token";

    /// <summary>Route for creating merchant session keys.</summary>
    internal const string MerchantSessionKeys = "/merchant-session-keys";

    /// <summary>Route for validating merchant session keys.</summary>
    internal const string MerchantSessionKeyValidation = "/merchant-session-keys/validation";

    /// <summary>Builds the route for retrieving a specific merchant session key.</summary>
    /// <param name="merchantSessionKey">The merchant session key to validate or inspect.</param>
    /// <returns>The relative API route for the merchant session key.</returns>
    internal static string MerchantSessionKey(string merchantSessionKey) => $"/merchant-session-keys/{PathSegment(merchantSessionKey, nameof(merchantSessionKey))}";

    /// <summary>Route for Apple Pay merchant session creation.</summary>
    internal const string ApplePaySession = "/applepay/sessions";

    /// <summary>Base route for card identifier operations.</summary>
    internal const string CardIdentifiers = "/card-identifiers";

    /// <summary>
    /// Builds the 3D Secure v2 challenge completion route for a specific transaction.
    /// After the customer completes the ACS challenge, POST the cRes to this endpoint.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for submitting the 3DS cRes.</returns>
    internal static string Transaction3DsChallenge(string transactionId) => $"/transactions/{PathSegment(transactionId, nameof(transactionId))}/3d-secure-challenge";

    /// <summary>Builds the instructions route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for transaction instructions.</returns>
    internal static string TransactionInstructions(string transactionId) => $"/transactions/{PathSegment(transactionId, nameof(transactionId))}/instructions";

    /// <summary>Builds the route to retrieve a specific transaction by its ID.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for retrieving a transaction.</returns>
    internal static string TransactionById(string transactionId) => $"/transactions/{PathSegment(transactionId, nameof(transactionId))}";

    /// <summary>Builds the security-code linking route for a card identifier.</summary>
    /// <param name="cardIdentifier">The card identifier token.</param>
    /// <returns>The relative API route for linking a security code.</returns>
    internal static string CardIdentifierSecurityCode(string cardIdentifier) => $"/card-identifiers/{PathSegment(cardIdentifier, nameof(cardIdentifier))}/security-code";

    /// <summary>Builds the route for a specific card identifier.</summary>
    /// <param name="cardIdentifier">The card identifier token.</param>
    /// <returns>The relative API route for the card identifier.</returns>
    internal static string CardIdentifierById(string cardIdentifier) => $"/card-identifiers/{PathSegment(cardIdentifier, nameof(cardIdentifier))}";

    private static string PathSegment(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);

        return Uri.EscapeDataString(value);
    }
}
